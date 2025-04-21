using RideTracker.Groups.Synchronization;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Infrastructure.Synchronization;
using RideTracker.Utilities;
using SQLite;

namespace RideTracker.Infrastructure.Synchronizers;

public class GroupsSynchronizer : EntitySynchronizer<Group, GroupResponse>
{
    public GroupsSynchronizer(RideTrackerHttpClient httpClient, ISQLiteAsyncConnection db, DbLogger<EntitySynchronizer<Group, GroupResponse>> logger, TimeProvider timeProvider, GroupUtils groupUtils) : base(httpClient, db, logger, timeProvider, groupUtils)
    {
    }

    protected override object GetUploadRequest(Group group)
    {
        return new CreateOrUpdateGroupRequest
        {
            GroupId = group.Id,
            GroupName = group.Name,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt,
            DeletedAt = group.DeletedAt
        };
    }

    protected override Group CreateEntityFromResponse(GroupResponse groupResponse)
    {
        return new Group
        {
            Id = groupResponse.Id,
            Name = groupResponse.GroupName,
            CreatedAt = groupResponse.CreatedAt,
            UpdatedAt = groupResponse.UpdatedAt,
            DeletedAt = groupResponse.DeletedAt,
            SynchronizedWithCloudAt = groupResponse.SynchronizedWithCloudAt,
            IsUploadedToCloud = true,
            IsManagedByCurrentUser = groupResponse.IsManagedByCurrentUser
        };
    }

    protected override void UpdateEntityFromResponse(GroupResponse groupResponse, Group existingGroup)
    {
        existingGroup.Name = groupResponse.GroupName;
        existingGroup.UpdatedAt = groupResponse.UpdatedAt;
        existingGroup.DeletedAt = groupResponse.DeletedAt;
        existingGroup.SynchronizedWithCloudAt = groupResponse.SynchronizedWithCloudAt;
        if(groupResponse.DeletedAt.HasValue)
        {
            existingGroup.IsCurrent = false;
            _groupUtils.ResetCurrentGroup();
        }
    }

    protected override Task<string> GetFetchEndpointAsync()
    {
        return Task.FromResult("api/groups");
    }

    protected override Task<string> GetUploadEndpointAsync(Group entity)
    {
        return Task.FromResult("api/groups");
    }

    protected override Task<bool> CanUploadAsync()
    {
        return Task.FromResult(true);
    }

    protected override Task<bool> CanFetchAsync()
    {
        return Task.FromResult(true);
    }

    protected async override Task<DateTime> GetLastSynchronizationTimeAsync()
    {
        return await _db.ExecuteScalarAsync<DateTime?>($"SELECT MAX(SynchronizedWithCloudAt) FROM Groups") ?? DateTime.MinValue;
    }
}
