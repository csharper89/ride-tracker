using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Infrastructure.Synchronizers;
using RideTracker.Invites;
using RideTracker.Rides.Synchronization;
using RideTracker.Vehicles.Synchronization;
using RideTracker.Vehicles.VehicleList;
using SQLite;

namespace RideTracker.Groups.Details;

[QueryProperty(nameof(GroupId), nameof(GroupId))]
public partial class GroupDetailsViewModel(ISQLiteAsyncConnection db, TimeProvider timeProvider, GroupsSynchronizer groupsSynchronizer, VehiclesSynchronizer vehiclesSynchronizer, RidesSynchronizer ridesSynchronizer, GroupUtils groupUtils, DbLogger<GroupDetailsViewModel> logger) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string? _groupId;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyCanExecuteChangedFor(nameof(InviteEmployeeCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    [NotifyPropertyChangedFor(nameof(IsEditMode))]
    private bool _isManagedByCurrentUser;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MakeCurrentCommand))]
    private bool _isCurrent;

    public bool CanMakeCurrent => !IsCurrent && IsSavedToDatabase;

    public bool CanSave => !IsSavedToDatabase || IsManagedByCurrentUser;

    public bool CanInvite => IsManagedByCurrentUser && IsSavedToDatabase;

    public bool CanDelete => IsManagedByCurrentUser && IsSavedToDatabase;

    private bool IsSavedToDatabase => !string.IsNullOrEmpty(GroupId);

    private Guid GroupIdParsed => new Guid(GroupId);

    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing GroupDetailsViewModel.");

        if (IsSavedToDatabase)
        {
            await LoadGroupDetailsAsync(GroupIdParsed);
            logger.LogInformation($"Loaded group details for GroupId: {GroupIdParsed}.");
        }
        else
        {
            IsEditMode = true;
            logger.LogInformation("EditMode enabled for new group.");
        }
    }

    private async Task LoadGroupDetailsAsync(Guid groupId)
    {
        var group = await db.Table<Group>().FirstAsync(g => g.Id == groupId);
        Name = group.Name;
        IsManagedByCurrentUser = group.IsManagedByCurrentUser;
        IsEditMode = group.IsManagedByCurrentUser;
        IsCurrent = group.IsCurrent;

        logger.LogInformation($"Group details loaded: Id={group.Id}, Name={group.Name}, ManagedByCurrentUser={group.IsManagedByCurrentUser}, IsCurrent={group.IsCurrent}");
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task SaveAsync()
    {
        logger.LogInformation("SaveAsync operation started.");

        if (IsSavedToDatabase)
        {
            var group = await db.Table<Group>().FirstAsync(g => g.Id == GroupIdParsed);
            group.Name = Name;
            group.UpdatedAt = timeProvider.GetUtcNow().DateTime;
            group.IsUploadedToCloud = false;
            await db.UpdateAsync(group);
            groupsSynchronizer.UploadSingleEntityToCloudAsync(group); // Fire and forget

            logger.LogInformation($"Group updated: Id={group.Id}, Name={group.Name}");
        }
        else
        {
            var noGroupsInDbYet = await db.Table<Group>().CountAsync() == 0;
            var group = new Group
            {
                Name = Name,
                Id = Guid.NewGuid(),
                CreatedAt = timeProvider.GetUtcNow().DateTime,
                IsManagedByCurrentUser = true,
                IsCurrent = noGroupsInDbYet
            };
            await db.InsertAsync(group);
            groupsSynchronizer.UploadSingleEntityToCloudAsync(group); // Fire and forget

            logger.LogInformation($"Group created: Id={group.Id}, Name={group.Name}");
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand(CanExecute = nameof(CanMakeCurrent))]
    public async Task MakeCurrent()
    {
        logger.LogInformation("MakeCurrent operation started.");

        IsBusy = true;
        try
        {
            await MakeCurrentInternalAsync();
        }
        finally
        {
            IsBusy = false;
        }

        await Shell.Current.GoToAsync("//" + nameof(VehicleListPage));
        logger.LogInformation($"MakeCurrent operation completed for GroupId: {GroupIdParsed}.");
    }

    private async Task MakeCurrentInternalAsync()
    {
        await Task.Delay(3000);
        await db.ExecuteAsync("UPDATE Groups SET IsCurrent = 0");

        var group = await db.Table<Group>().FirstAsync(g => g.Id == GroupIdParsed);
        group.IsCurrent = true;
        await db.UpdateAsync(group);
        groupUtils.ResetCurrentGroup();
        await vehiclesSynchronizer.FetchEntitiesFromCloudAsync();
        await ridesSynchronizer.FetchEntitiesFromCloudAsync();

        logger.LogInformation($"Group set as current: Id={group.Id}, Name={group.Name}");
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task DeleteAsync()
    {
        logger.LogInformation("DeleteAsync operation started.");

        var group = await db.Table<Group>().FirstAsync(g => g.Id == GroupIdParsed);
        group.DeletedAt = timeProvider.GetUtcNow().DateTime;
        await db.UpdateAsync(group);
        groupUtils.ResetCurrentGroup();
        groupsSynchronizer.UploadSingleEntityToCloudAsync(group); // Fire and forget

        await Shell.Current.GoToAsync("..");

        logger.LogInformation($"Group deleted: Id={group.Id}, Name={group.Name}");
    }

    [RelayCommand(CanExecute = nameof(CanInvite))]
    public async Task InviteEmployeeAsync()
    {
        logger.LogInformation("InviteEmployeeAsync operation started.");

        var parameters = new Dictionary<string, object> { { "GroupId", new Guid(GroupId!) } };
        await Shell.Current.GoToAsync(nameof(CreateInvitePage), parameters);

        logger.LogInformation($"Navigated to CreateInvitePage with GroupId: {GroupId}");
    }
}