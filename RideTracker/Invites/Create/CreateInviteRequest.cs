namespace RideTracker.Invites;

public class CreateInviteRequest
{
    public Guid GroupId { get; init; }
    public bool IsAdmin { get; init; }
}
