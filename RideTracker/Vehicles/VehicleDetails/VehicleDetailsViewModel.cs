using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Resources.Languages;
using RideTracker.Vehicles.Synchronization;
using SQLite;

namespace RideTracker.Vehicles.VehicleDetails;

[QueryProperty(nameof(VehicleId), nameof(VehicleId))]
public partial class VehicleDetailsViewModel(ISQLiteAsyncConnection db, TimeProvider timeProvider, GroupUtils groupUtils, AlertsService alertsService, VehiclesSynchronizer synchronizer, DbLogger<VehicleDetailsViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private string _vehicleId;

    [ObservableProperty]
    private string _pricePerUnitOfTimeLabel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _quickSaveButton1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _quickSaveButton2;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _unitOfTime;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _pricePerUnitOfTime;

    private bool IsSavedToDatabase => !string.IsNullOrEmpty(VehicleId);

    private string QuickSaveButtons => $"{QuickSaveButton1},{QuickSaveButton2}";

    private Guid VehicleIdParsed => new Guid(VehicleId);

    public bool CanSave => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(UnitOfTime) && decimal.TryParse(PricePerUnitOfTime, out var price) && price > 0;

    public bool CanDelete => IsSavedToDatabase;

    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing VehicleDetailsViewModel...");

        QuickSaveButton1 = "5";
        QuickSaveButton2 = "10";
        if (IsSavedToDatabase)
        {
            logger.LogInformation($"Loading vehicle details for VehicleId: {VehicleId}.");

            var vehicle = await db.Table<Vehicle>().FirstAsync(v => v.Id == VehicleIdParsed);
            Name = vehicle.Name;
            PricePerUnitOfTime = vehicle.PricePerUnitOfTime.ToString();
            UnitOfTime = vehicle.UnitOfTimeInMinutes.ToString();

            var quickSaveButtons = vehicle.QuickSaveButtons.Split(',');
            QuickSaveButton1 = quickSaveButtons[0];
            QuickSaveButton2 = quickSaveButtons[1];

            logger.LogInformation("Vehicle details loaded successfully.");
        }
        PricePerUnitOfTimeLabel = string.Format(AppResources.Vehicles_Price, UnitOfTime);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task DeleteAsync()
    {
        logger.LogInformation("DeleteAsync operation started for VehicleId: {VehicleId}.");

        var userConfirmed = await alertsService.ConfirmAsync(AppResources.Vehicles_DeleteConfirmationTitle, AppResources.Vehicles_DeleteConfirmationMessage);
        if (!userConfirmed)
        {
            logger.LogInformation("Delete operation canceled by user.");
            return;
        }

        var vehicle = await db.Table<Vehicle>().FirstAsync(v => v.Id == VehicleIdParsed);
        vehicle.DeletedAt = timeProvider.GetUtcNow().DateTime;
        vehicle.IsUploadedToCloud = false;
        await db.UpdateAsync(vehicle);
        synchronizer.UploadSingleEntityToCloudAsync(vehicle); // Fire and forget

        await Shell.Current.GoToAsync("..");
        logger.LogInformation($"Vehicle {VehicleId} deleted and navigated back.");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        logger.LogInformation("Save operation started.");

        if (IsSavedToDatabase)
        {
            logger.LogInformation($"Updating vehicle with VehicleId: {VehicleId}.");

            var vehicle = await db.Table<Vehicle>().FirstAsync(v => v.Id == VehicleIdParsed);
            vehicle.Name = Name;
            vehicle.PricePerUnitOfTime = int.Parse(PricePerUnitOfTime);
            vehicle.UnitOfTimeInMinutes = int.Parse(UnitOfTime);
            vehicle.IsUploadedToCloud = false;
            vehicle.QuickSaveButtons = QuickSaveButtons;
            vehicle.UpdatedAt = timeProvider.GetUtcNow().DateTime;
            await db.UpdateAsync(vehicle);
            synchronizer.UploadSingleEntityToCloudAsync(vehicle); // Fire and forget

            logger.LogInformation($"Vehicle {VehicleId} updated successfully.");
        }
        else
        {
            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Name = Name,
                PricePerUnitOfTime = int.Parse(PricePerUnitOfTime),
                UnitOfTimeInMinutes = int.Parse(UnitOfTime),
                QuickSaveButtons = QuickSaveButtons,
                GroupId = (await groupUtils.GetCurrentGroupIdAsync()).Value,
                CreatedAt = timeProvider.GetUtcNow().DateTime
            };
            await db.InsertAsync(vehicle);
            synchronizer.UploadSingleEntityToCloudAsync(vehicle); // Fire and forget

            logger.LogInformation($"New vehicle created with VehicleId: {vehicle.Id}.");
        }

        await Shell.Current.GoToAsync("..");
        logger.LogInformation("Save operation completed and navigated back.");
    }

    partial void OnUnitOfTimeChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            PricePerUnitOfTimeLabel = string.Format(AppResources.Vehicles_Price, value);
            logger.LogInformation($"UnitOfTime changed to {value}, updated PricePerUnitOfTimeLabel.");
        }
    }
}