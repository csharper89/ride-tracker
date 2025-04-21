using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Authentication.Services;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Resources.Languages;
using RideTracker.Rides.DaysSummary;
using RideTracker.Rides.Synchronization;
using SQLite;

namespace RideTracker.Rides.Details;

[QueryProperty(nameof(RideId), nameof(RideId))]
[QueryProperty(nameof(RideDetails), nameof(RideDetails))]
public partial class RideDetailsViewModel(ISQLiteAsyncConnection db, TimeProvider timeProvider, AlertsService alertsService, IAuthenticationService authService, RideHistoryHelper rideHistoryHelper, RidesSynchronizer synchronizer, DbLogger<RideDetailsViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private string _vehicleName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _rideDurationInMinutes;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _pricePerUnitOfTime;

    [ObservableProperty]
    private string _cost;

    [ObservableProperty]
    private string _rideId;

    [ObservableProperty]
    private bool _canDelete;

    [ObservableProperty]
    private RideDetailsDto _rideDetails;

    [ObservableProperty]
    private string _pricePerUnitOfTimeLabel;

    private int _unitOfTimeInMinutes;

    private Guid RideIdParsed => new Guid(RideId);

    private bool IsSavedToDatabase => !string.IsNullOrEmpty(RideId);

    private bool CanSave => int.TryParse(Cost, out var cost) && cost > 0;

    public Task InitializeAsync() => IsSavedToDatabase ? LoadSavedRide() : InitializeUnsavedRide();

    private async Task InitializeUnsavedRide()
    {
        logger.LogInformation("Initializing unsaved ride.");

        var vehicleId = RideDetails.VehicleId;
        var vehicle = await db.Table<Vehicle>().FirstAsync(v => v.Id == vehicleId);
        VehicleName = vehicle.Name;
        PricePerUnitOfTime = vehicle.PricePerUnitOfTime.ToString();
        _unitOfTimeInMinutes = vehicle.UnitOfTimeInMinutes;
        var minutesCountRounded = (int)Math.Ceiling(RideDetails.EstimatedRideInMinutes / (decimal)_unitOfTimeInMinutes) * _unitOfTimeInMinutes;
        RideDurationInMinutes = minutesCountRounded.ToString();
        Cost = (minutesCountRounded / _unitOfTimeInMinutes * vehicle.PricePerUnitOfTime).ToString();
        PricePerUnitOfTimeLabel = string.Format(AppResources.Vehicles_Price, vehicle.UnitOfTimeInMinutes);

        logger.LogInformation($"Unsaved ride initialized: VehicleName={VehicleName}, RideDurationInMinutes={RideDurationInMinutes}, Cost={Cost}");
    }

    private async Task LoadSavedRide()
    {
        logger.LogInformation("Loading saved ride.");

        var ride = await db.Table<Ride>().FirstAsync(r => r.Id == RideIdParsed);
        var vehicle = await db.Table<Vehicle>().FirstAsync(v => v.Id == ride.VehicleId);
        VehicleName = vehicle.Name;
        _unitOfTimeInMinutes = ride.UnitOfTimeInMinutes;
        PricePerUnitOfTime = ride.PricePerUnitOfTime.ToString();
        RideDurationInMinutes = ride.RideDurationInMinutes.ToString();
        PricePerUnitOfTimeLabel = string.Format(AppResources.Vehicles_Price, vehicle.UnitOfTimeInMinutes);
        Cost = ride.Cost.ToString();
        CanDelete = true;

        logger.LogInformation($"Saved ride loaded: VehicleName={VehicleName}, RideDurationInMinutes={RideDurationInMinutes}, Cost={Cost}");
    }

    partial void OnPricePerUnitOfTimeChanged(string value) => UpdateCost(value, RideDurationInMinutes);

    partial void OnRideDurationInMinutesChanged(string value) => UpdateCost(PricePerUnitOfTime, value);

    private void UpdateCost(string pricePerUnitOfTime, string rideDurationInMinutes)
    {
        var durationIsValid = int.TryParse(rideDurationInMinutes, out var rideDurationInMinutesParsed);
        var priceIsValid = int.TryParse(pricePerUnitOfTime, out var pricePerUnitOfTimeParsed);

        if (durationIsValid && priceIsValid)
        {
            Cost = (rideDurationInMinutesParsed / _unitOfTimeInMinutes * pricePerUnitOfTimeParsed).ToString();
            logger.LogInformation($"Cost updated to {Cost}.");
        }
    }

    [RelayCommand]
    public async Task Delete()
    {
        logger.LogInformation("Delete operation started.");

        if (!await alertsService.ConfirmAsync(AppResources.RideDetails_ConfirmDeleteTitle, AppResources.RideDetails_ConfirmDeleteMessage))
        {
            logger.LogInformation("Delete operation cancelled by user.");
            return;
        }

        var ride = await db.Table<Ride>().FirstAsync(r => r.Id == RideIdParsed);
        ride.DeletedAt = timeProvider.GetUtcNow().DateTime;
        ride.IsUploadedToCloud = false;
        await db.UpdateAsync(ride);
        rideHistoryHelper.UpdateSummariesAsync();
        synchronizer.UploadSingleEntityToCloudAsync(ride); // Fire and forget
        await Shell.Current.GoToAsync("..");

        logger.LogInformation($"Ride {RideId} deleted and navigated back.");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        logger.LogInformation("Save operation started.");

        if (IsSavedToDatabase)
        {
            await UpdateRide();
        }
        else
        {
            await CreateRide();
        }

        await Shell.Current.GoToAsync("..");
        logger.LogInformation("Save operation completed and navigated back.");
    }

    private async Task UpdateRide()
    {
        logger.LogInformation($"Updating ride with RideId: {RideId}.");

        try
        {
            var ride = await db.Table<Ride>().FirstAsync(r => r.Id == RideIdParsed);
            ride.IsUploadedToCloud = false;
            ride.Cost = int.Parse(Cost);
            ride.RideDurationInMinutes = int.Parse(RideDurationInMinutes);
            ride.PricePerUnitOfTime = int.Parse(PricePerUnitOfTime);
            ride.UnitOfTimeInMinutes = _unitOfTimeInMinutes;
            ride.UpdatedAt = timeProvider.GetUtcNow().DateTime;
            await db.UpdateAsync(ride);
            rideHistoryHelper.UpdateSummariesAsync();
            synchronizer.UploadSingleEntityToCloudAsync(ride); // Fire and forget

            logger.LogInformation($"Ride {RideId} updated successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error updating ride {RideId}.");
        }
    }

    private async Task CreateRide()
    {
        var rideId = Guid.NewGuid();
        logger.LogInformation($"Creating new ride with RideId: {rideId}.");

        var ride = new Ride
        {
            Id = rideId,
            VehicleId = RideDetails.VehicleId,
            StartedAt = RideDetails.StartedAt,
            StoppedAt = RideDetails.StoppedAt,
            VehicleName = VehicleName,
            CreatedAt = timeProvider.GetUtcNow().DateTime,
            CreatedBy = authService.GetCurrentUserEmail(),
            RideDurationInMinutes = int.Parse(RideDurationInMinutes),
            PricePerUnitOfTime = int.Parse(PricePerUnitOfTime),
            UnitOfTimeInMinutes = _unitOfTimeInMinutes,
            Cost = int.Parse(Cost)
        };
        await db.InsertAsync(ride);
        synchronizer.UploadSingleEntityToCloudAsync(ride); // Fire and forget
        rideHistoryHelper.UpdateSummariesAsync();
        await Shell.Current.GoToAsync("..");

        logger.LogInformation($"Ride {rideId} created successfully and navigated back.");
    }
}