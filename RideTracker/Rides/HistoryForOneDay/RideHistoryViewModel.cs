using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Rides.Details;
using RideTracker.Rides.HistoryForOneDay;
using RideTracker.Rides.Synchronization;
using SQLite;

namespace RideTracker.Rides.History;

[QueryProperty(nameof(Date), nameof(Date))]
public partial class RideHistoryViewModel(ISQLiteAsyncConnection db, RidesSynchronizer ridesSynchronizer, GroupUtils groupUtils, DbLogger<RideDetailsViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private List<RideSummary> _rideSummaries;

    [ObservableProperty]
    private DateTime _date;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _dateFormatted;

    [ObservableProperty]
    private int _sum;

    public async Task InitializeAsync()
    {
        logger.LogInformation($"Initializing RideHistoryViewModel for date: {Date}.");
        await LoadDataFromDbAsync();
    }

    private async Task LoadDataFromDbAsync()
    {
        logger.LogInformation("Loading data from database...");

        try
        {
            var groupId = await groupUtils.GetCurrentGroupIdAsync();
            var startOfDayUtc = DateTime.SpecifyKind(Date, DateTimeKind.Local).ToUniversalTime();
            var endOfDayUtc = Date.AddDays(1).ToUniversalTime();

            RideSummaries = await db.QueryAsync<RideSummary>(@"
                SELECT r.Id, r.VehicleName, r.CreatedAt, r.IsUploadedToCloud, 
                       r.PricePerUnitOfTime, r.RideDurationInMinutes, r.Cost, r.UnitOfTimeInMinutes
                FROM Rides r
                INNER JOIN Vehicles v ON r.VehicleId = v.Id
                INNER JOIN Groups g ON v.GroupId = g.Id
                WHERE r.DeletedAt IS NULL AND g.Id = ? AND r.CreatedAt > ? AND r.CreatedAt < ?", groupId, startOfDayUtc, endOfDayUtc);

            DateFormatted = Date.ToString("dd.MM.yyyy");
            Sum = RideSummaries.Sum(x => x.Cost);

            logger.LogInformation($"Data loaded: {RideSummaries.Count} ride summaries found for {DateFormatted}, total cost: {Sum}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading data from database.");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        logger.LogInformation("Refreshing RideHistoryViewModel...");
        try
        {
            IsBusy = true;
            await ridesSynchronizer.FetchEntitiesFromCloudAsync();
            logger.LogInformation("Entities fetched from cloud.");
            await LoadDataFromDbAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during initialization.");
        }
        finally
        {
            IsBusy = false;
            logger.LogInformation("RideHistoryViewModel refresh completed.");
        }
    }

    [RelayCommand]
    private async Task OpenRideDetailsAsync(RideSummary ride)
    {
        logger.LogInformation($"Opening ride details for ride ID: {ride.Id}.");
        await Shell.Current.GoToAsync($"{nameof(RideDetailsPage)}?RideId={ride.Id}");
    }
}