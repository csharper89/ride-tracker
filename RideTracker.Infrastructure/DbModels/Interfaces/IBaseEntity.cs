namespace RideTracker.Infrastructure.DbModels.Interfaces;

public interface IBaseEntity
{
    Guid Id { get; init; }
    DateTime CreatedAt { get; init; }
    DateTime? SynchronizedWithCloudAt { get; set; }
    bool IsUploadedToCloud { get; set; }
}
