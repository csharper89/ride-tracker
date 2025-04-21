using RideTracker.Infrastructure.DbModels.Interfaces;
using SQLite;

namespace RideTracker.Infrastructure.DbModels;

[Table("Groups")]
public class Group : IBaseEntity, IModifiableEntity
{
    [PrimaryKey]
    public Guid Id { get; init; }
    public string Name { get; set; }    
    public DateTime CreatedAt { get; init; }
    public DateTime? SynchronizedWithCloudAt { get; set; }
    public bool IsManagedByCurrentUser { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsUploadedToCloud { get; set; }
}
