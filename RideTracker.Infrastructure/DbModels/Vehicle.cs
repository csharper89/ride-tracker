using RideTracker.Infrastructure.DbModels.Interfaces;
using SQLite;

namespace RideTracker.Infrastructure.DbModels;

[Table("Vehicles")]
public class Vehicle : IBaseEntity, IModifiableEntity
{
    [PrimaryKey]
    public Guid Id { get; init; }
    public string Name { get; set; }
    public string QuickSaveButtons { get; set; }
    public int PricePerUnitOfTime { get; set; }
    public int UnitOfTimeInMinutes { get; set; }
    public int Order { get; set; }
    public DateTime? UpdatedAt { get; set; }    
    public DateTime CreatedAt { get; init; }
    public DateTime RideStartedAt { get; init; }
    public DateTime? SynchronizedWithCloudAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid GroupId { get; set; }
    public bool IsUploadedToCloud { get; set; }
}