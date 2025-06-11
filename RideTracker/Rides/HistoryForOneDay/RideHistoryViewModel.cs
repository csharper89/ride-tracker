using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
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

    [ObservableProperty]
    private int _salary;

    [ObservableProperty]
    private bool _isSalaryVisible;

    private readonly int _baseSalaryPerDay = 800;
    private readonly int _benefitsStartAt = 4000;

    public async Task InitializeAsync()
    {
        logger.LogInformation($"Initializing RideHistoryViewModel for date: {Date}.");

        try
        {
            IsBusy = true;
            await LoadDataFromDbAsync();
            logger.LogInformation("Data loaded from database.");

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
            logger.LogInformation("RideHistoryViewModel initialization completed.");
        }
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
            Salary = CalculateSalary(Sum);
            IsSalaryVisible = Salary > _baseSalaryPerDay && await groupUtils.IsUserManagingCurrentGroupAsync();

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
        await InitializeAsync();
    }

    [RelayCommand]
    private async Task OpenRideDetailsAsync(RideSummary ride)
    {
        logger.LogInformation($"Opening ride details for ride ID: {ride.Id}.");
        await Shell.Current.GoToAsync($"{nameof(RideDetailsPage)}?RideId={ride.Id}");
    }

    private int CalculateSalary(int sum)
    {
        if(sum <= _baseSalaryPerDay)
        {
            return sum;
        }

        if(sum < _benefitsStartAt)
        {
            return _baseSalaryPerDay;
        }

        var benefits = 0;
        var currentLevel = _benefitsStartAt;

        while (currentLevel <= sum)
        {
            benefits += 100;
            currentLevel += 1000;
        }

        return _baseSalaryPerDay + benefits;
    }
}