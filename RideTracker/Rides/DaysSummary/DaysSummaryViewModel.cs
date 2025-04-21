using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
using RideTracker.Rides.DaysSummary;
using RideTracker.Rides.History;
using RideTracker.Rides.Synchronization;

namespace RideTracker.Rides.ListOfDays;

public partial class DaysSummaryViewModel(RideHistoryHelper helper, RidesSynchronizer ridesSynchronizer, DbLogger<DaysSummaryViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    private List<SummaryForDay> _summaries;

    [ObservableProperty]
    private bool _isBusy;

    public async Task LoadSummariesAsync()
    {
        logger.LogInformation("Loading summaries...");
        try
        {
            Summaries = await helper.GetDatesSummaryAsync();
            logger.LogInformation("Summaries loaded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading summaries.");
        }
    }

    [RelayCommand]
    private async Task OpenHistoryForOneDayAsync(SummaryForDay summary)
    {
        logger.LogInformation($"Opening history for one day with date: {summary.Date}.");
        if (summary.TotalSumForDay == 0)
        {
            logger.LogInformation("TotalSumForDay is 0. Skipping navigation.");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(nameof(HistoryForOneDayPage), new Dictionary<string, object>
            {
                { "Date", summary.Date }
            });
            logger.LogInformation($"Navigated to HistoryForOneDayPage for date: {summary.Date}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error navigating to HistoryForOneDayPage for date: {summary.Date}.");
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        logger.LogInformation("Refreshing data...");
        IsBusy = true;
        try
        {
            await ridesSynchronizer.FetchEntitiesFromCloudAsync();
            logger.LogInformation("Entities fetched from cloud successfully.");
            await helper.UpdateSummariesAsync();
            logger.LogInformation("Summaries updated successfully.");
            await LoadSummariesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during refresh.");
        }
        finally
        {
            IsBusy = false;
            logger.LogInformation("Refresh completed.");
        }
    }
}