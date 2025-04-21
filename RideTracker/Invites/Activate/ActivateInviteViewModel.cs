using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Infrastructure.Synchronizers;
using RideTracker.Rides.Synchronization;
using RideTracker.Vehicles.Synchronization;
using RideTracker.Vehicles.VehicleList;
using SQLite;

namespace RideTracker.Invites.Activate;

public partial class ActivateInviteViewModel(RideTrackerHttpClient httpClient, GroupsSynchronizer groupsSynchronizer, VehiclesSynchronizer vehiclesSynchronizer, RidesSynchronizer ridesSynchronizer, ISQLiteAsyncConnection db, GroupUtils groupUtils, DbLogger<ActivateInviteViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ActivateInviteCommand))]
    private string _inviteCode;

    [ObservableProperty]
    private bool _errorOccurred;

    private bool CanActivateInvite => !string.IsNullOrEmpty(InviteCode) && InviteCode.Length == 4;

    [RelayCommand(CanExecute = nameof(CanActivateInvite))]
    private async Task ActivateInviteAsync()
    {
        logger.LogInformation("ActivateInviteAsync operation started.");
        ErrorOccurred = false;

        var request = new ActivateInviteRequest
        {
            InviteCode = InviteCode
        };

        try
        {
            logger.LogInformation("Sending invite activation request...");
            var response = await httpClient.PostAsync<ActivateInviteResponse>("api/invites/activate", request);
            logger.LogInformation("Invite activation successful. Fetching new entities...");
            await groupsSynchronizer.FetchEntitiesFromCloudAsync();
            await UpdateCurrentGroupAsync(response.GroupId);
            await FetchNewEntitiesAsync();

            await Shell.Current.GoToAsync("//" + nameof(VehicleListPage));
            logger.LogInformation("Navigation to VehicleListPage completed.");
        }
        catch (Exception ex)
        {
            ErrorOccurred = true;
            logger.LogError(ex, "Error during invite activation.");
        }
    }

    private async Task UpdateCurrentGroupAsync(Guid groupId)
    {
        logger.LogInformation($"Updating current group to GroupId: {groupId}");
        await db.ExecuteAsync("UPDATE Groups SET IsCurrent = 0");
        var group = await db.Table<Group>().FirstAsync(g => g.Id == groupId);
        group.IsCurrent = true;
        await db.UpdateAsync(group);
        groupUtils.ResetCurrentGroup();
        logger.LogInformation($"Group {group.Name} is now set as current.");
    }

    private async Task FetchNewEntitiesAsync()
    {
        logger.LogInformation("Fetching new entities from the cloud...");        
        await vehiclesSynchronizer.FetchEntitiesFromCloudAsync();
        await ridesSynchronizer.FetchEntitiesFromCloudAsync();
        logger.LogInformation("New entities fetched from the cloud successfully.");
    }
}