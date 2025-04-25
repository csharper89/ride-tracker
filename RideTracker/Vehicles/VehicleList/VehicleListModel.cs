using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Authentication.Services;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Resources.Languages;
using RideTracker.Rides.DaysSummary;
using RideTracker.Rides.Details;
using RideTracker.Rides.ListOfDays;
using RideTracker.Rides.Synchronization;
using RideTracker.Vehicles.Synchronization;
using RideTracker.Vehicles.VehicleDetails;
using SQLite;
using System.Collections.ObjectModel;
using ITimer = RideTracker.Utilities.ITimer;

namespace RideTracker.Vehicles.VehicleList;

public partial class VehicleListModel : ObservableObject
{
    private readonly TimeProvider _timeProvider;
    private readonly GroupUtils _groupUtils;
    private readonly ISQLiteAsyncConnection _db;
    private readonly IAuthenticationService _authenticationService;
    private readonly RideHistoryHelper _rideHistoryHelper;
    private readonly RidesSynchronizer _ridesSynchronizer;
    private readonly VehiclesSynchronizer _vehiclesSynchronizer;
    private readonly DbLogger<VehicleListModel> _logger;

    [ObservableProperty]
    private ObservableCollection<VehicleModel> _vehicles;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private double _vehicleTitleFontSize;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateVehicleCommand))]
    private bool _isGroupManagedByCurrentUser;

    private DateTime UtcNow => _timeProvider.GetUtcNow().DateTime;

    public VehicleListModel(TimeProvider timeProvider, ITimer timer, GroupUtils groupUtils, ISQLiteAsyncConnection db, IAuthenticationService authenticationService, RideHistoryHelper rideHistoryHelper, RidesSynchronizer ridesSynchronizer, VehiclesSynchronizer vehiclesSynchronizer, DbLogger<VehicleListModel> logger)
    {
        _timeProvider = timeProvider;
        _groupUtils = groupUtils;
        _db = db;
        _authenticationService = authenticationService;
        _rideHistoryHelper = rideHistoryHelper;
        _ridesSynchronizer = ridesSynchronizer;
        _vehiclesSynchronizer = vehiclesSynchronizer;
        _logger = logger;

        Vehicles = new ObservableCollection<VehicleModel>();
        VehicleTitleFontSize = GetFontSizeForVehicleTitle();
        timer.ExecuteActionEverySecond(UpdateElapsedTimeForAllVehicles);

        _logger.LogInformation("VehicleListModel initialized.");
    }

    public async Task Load()
    {
        _logger.LogInformation("Loading vehicles...");
        try
        {
            var currentGroupId = await _groupUtils.GetCurrentGroupIdAsync();
            if(!currentGroupId.HasValue)
            {
                _logger.LogInformation("Current group ID is null. Clearing vehicles list.");
                Vehicles = new ObservableCollection<VehicleModel>();
                return;
            }

            var vehicles = await _db.Table<Vehicle>()
                .Where(v => v.GroupId == currentGroupId && v.DeletedAt == null)
                .ToListAsync();

            var models = vehicles.Select(v => new VehicleModel(v)).ToList();
            foreach (var vehicleModel in Vehicles)
            {
                var v = models.FirstOrDefault(c => c.Id == vehicleModel.Id);
                if (v != null)
                {
                    v.RestoreState(vehicleModel);
                }
            }
            Vehicles = new ObservableCollection<VehicleModel>(models);
            IsGroupManagedByCurrentUser = await _groupUtils.IsUserManagingCurrentGroupAsync();
            _logger.LogInformation("Vehicles loaded successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading vehicles.");
        }
    }

    [RelayCommand]
    private void StartRide(Guid vehicleId)
    {
        _logger.LogInformation($"Starting ride for vehicle ID: {vehicleId}.");
        var vehicleModel = GetVehicleModel(vehicleId);
        if (!vehicleModel.RideInProgress)
        {
            vehicleModel.StartRide(UtcNow);
        }
        vehicleModel.IncreaseEstimatedRideTime(vehicleModel.UnitOfTimeInMinutes);
        UpdateElapsedTimeForAllVehicles();

        _logger.LogInformation($"Ride started for vehicle ID: {vehicleId}.");
    }

    [RelayCommand(CanExecute = nameof(IsGroupManagedByCurrentUser))]
    private async Task OpenVehicleDetails(Guid vehicleId)
    {
        _logger.LogInformation($"Opening vehicle details for vehicle ID: {vehicleId}.");
        await Shell.Current.GoToAsync($"{nameof(VehicleDetailsPage)}?VehicleId={vehicleId}");
    }

    [RelayCommand(CanExecute = nameof(IsGroupManagedByCurrentUser))]
    private async Task CreateVehicleAsync()
    {
        _logger.LogInformation("Navigating to create vehicle page.");
        await Shell.Current.GoToAsync($"{nameof(VehicleDetailsPage)}");
    }

    [RelayCommand]
    private async Task GoToHistoryPageAsync()
    {
        _logger.LogInformation("Navigating to history page.");
        await Shell.Current.GoToAsync($"{nameof(DaysSummaryPage)}");
    }

    [RelayCommand]
    private async Task StopRide(Guid vehicleId)
    {
        _logger.LogInformation($"Stopping ride for vehicle ID: {vehicleId}.");
        var vehicleModel = GetVehicleModel(vehicleId);
        if(!vehicleModel.RideInProgress)
        {
            _logger.LogInformation("Ride is not in progress. Skipping stop ride action.");
            return;
        }
        var rideDetails = new RideDetailsDto(vehicleModel.Id, vehicleModel.RideStartedAtUtc!.Value, _timeProvider.GetUtcNow().DateTime, vehicleModel.EstimatedRideTimeInMinutes!.Value);
        await vehicleModel.EndRide(_timeProvider.GetUtcNow().DateTime);

        UpdateElapsedTimeForAllVehicles();
        await GoToSaveRidePage(rideDetails);

        _logger.LogInformation($"Ride stopped and details page opened for vehicle ID: {vehicleId}.");
    }

    [RelayCommand]
    private async Task QuickSave1Async(Guid vehicleId)
    {
        await QuickSaveAsync(vehicleId, 0);
    }

    [RelayCommand]
    private async Task QuickSave2Async(Guid vehicleId)
    {
        await QuickSaveAsync(vehicleId, 1);
    }

    private async Task QuickSaveAsync(Guid vehicleId, int buttonIndex)
    {
        _logger.LogInformation($"Quick saving ride for vehicle ID: {vehicleId} with button index: {buttonIndex}.");

        var vehicle = await _db.Table<Vehicle>().FirstAsync(v => v.Id == vehicleId);
        var quickSaveButtons = vehicle.QuickSaveButtons.Split(',');
        var rideDurationInMinutes = int.Parse(quickSaveButtons[buttonIndex]);
        var ride = new Ride
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicleId,
            StartedAt = _timeProvider.GetUtcNow().DateTime,
            StoppedAt = _timeProvider.GetUtcNow().DateTime,
            VehicleName = vehicle.Name,
            CreatedAt = _timeProvider.GetUtcNow().DateTime,
            CreatedBy = _authenticationService.GetCurrentUserEmail(),
            RideDurationInMinutes = rideDurationInMinutes,
            PricePerUnitOfTime = vehicle.PricePerUnitOfTime,
            UnitOfTimeInMinutes = vehicle.UnitOfTimeInMinutes,
            Cost = rideDurationInMinutes / vehicle.UnitOfTimeInMinutes * vehicle.PricePerUnitOfTime
        };
        await _db.InsertAsync(ride);

        _rideHistoryHelper.UpdateSummariesAsync();
        _ridesSynchronizer.UploadSingleEntityToCloudAsync(ride); // Fire and forget

        var message = $"{AppResources.Vehicles_RideSaved}. {ride.VehicleName}, {ride.RideDurationInMinutes} {AppResources.Minutes} X {ride.PricePerUnitOfTime} = {ride.Cost}";
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var toast = Toast.Make(message, ToastDuration.Long, textSize: 18);
        await toast.Show(cts.Token);

        _logger.LogInformation($"Quick save completed for vehicle ID: {vehicleId} with duration: {rideDurationInMinutes} minutes.");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _logger.LogInformation("Refreshing vehicle data.");

        IsBusy = true;
        try
        {
            await _vehiclesSynchronizer.FetchEntitiesFromCloudAsync();
            await Load();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing vehicle data.");
        }
        finally
        {
            IsBusy = false;
            _logger.LogInformation("Vehicle data refresh completed.");
        }
    }

    private async Task GoToSaveRidePage(RideDetailsDto rideDetails)
    {
        var parameters = new Dictionary<string, object> { { "RideDetails", rideDetails } };
        await Shell.Current.GoToAsync(nameof(RideDetailsPage), parameters);
        _logger.LogInformation($"Navigated to RideDetailsPage for ride. Vehicle ID: {rideDetails.VehicleId}");
    }

    private VehicleModel GetVehicleModel(Guid vehicleId) => Vehicles.First(v => v.Id == vehicleId);

    private void UpdateElapsedTimeForAllVehicles()
    {
        foreach (var vehicle in Vehicles)
        {
            vehicle.UpdateElapsedTime(UtcNow);
        }
    }

    private double GetFontSizeForVehicleTitle()
    {
        const int FontSizeLarge = 23;
        const int FontSizeDefault = 20;

        return Platform.CurrentActivity.Resources.Configuration.FontScale > 1
            ? FontSizeDefault
            : FontSizeLarge;
    }
}