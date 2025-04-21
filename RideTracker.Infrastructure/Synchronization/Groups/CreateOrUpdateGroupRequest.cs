namespace RideTracker.Groups.Synchronization;

public class CreateOrUpdateGroupRequest
{
    public Guid GroupId { get; init; }
    public string GroupName { get; init; }
    public DateTime? DeletedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
