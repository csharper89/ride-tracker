using RideTracker.Infrastructure;

namespace RideTracker.Groups.Synchronization;

public class GroupResponse : ApiResponseBase
{
    public string GroupName { get; init; }
    public DateTime? DeletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime SynchronizedWithCloudAt { get; init; }
    public bool IsManagedByCurrentUser { get; init; }
}
