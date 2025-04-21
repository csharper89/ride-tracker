using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Groups.Details;
using RideTracker.Groups.ListPage;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Invites.Activate;
using SQLite;
using System.Collections.ObjectModel;

namespace RideTracker.Groups.List;

public partial class GroupListModel(ISQLiteAsyncConnection db, DbLogger<GroupListModel> logger) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GroupListItem> groups;

    [ObservableProperty]
    private bool _noGroupsAvailable;

    public GroupListItem SelectedGroup { get; set; }

    public async Task LoadGroupsAsync()
    {
        logger.LogInformation("Loading groups...");

        try
        {
            var groups = await db.Table<Group>().Where(g => g.DeletedAt == null).ToListAsync();
            var items = groups.Select(g => new GroupListItem
            {
                Id = g.Id,
                Name = g.Name,
                IsCurrent = g.IsCurrent,
            }).ToList();
            Groups = new ObservableCollection<GroupListItem>(items);
            NoGroupsAvailable = Groups.Count == 0;

            logger.LogInformation($"Loaded {Groups.Count} groups.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading groups.");
        }
    }

    [RelayCommand]
    public async Task GoToCreateGroupPageAsync()
    {
        logger.LogInformation("Navigating to Create Group Page...");
        await Shell.Current.GoToAsync(nameof(GroupDetailsPage));
    }

    [RelayCommand]
    public async Task GoToActivateInvitePageAsync()
    {
        logger.LogInformation("Navigating to Activate Invite Page...");
        await Shell.Current.GoToAsync(nameof(ActivateInvitePage));
    }

    [RelayCommand]
    public async Task OpenGroupDetails()
    {
        if (SelectedGroup == null)
        {
            logger.LogWarning("OpenGroupDetails called but SelectedGroup is null.");
            return;
        }

        logger.LogInformation($"Navigating to Group Details Page for GroupId: {SelectedGroup.Id}...");
        await Shell.Current.GoToAsync($"{nameof(GroupDetailsPage)}?GroupId={SelectedGroup.Id}");
    }
}