using RideTracker.Infrastructure.DbModels.Interfaces;
using SQLite;

namespace RideTracker.Infrastructure.DbModels;

[Table("Rides")]
public class Ride : IBaseEntity, IModifiableEntity
{
    [PrimaryKey]
    public Guid Id { get; init; }
    public Guid VehicleId { get; set; }
    public string VehicleName { get; set; }
    public int RideDurationInMinutes { get; set; }
    public int PricePerUnitOfTime { get; set; }
    public int UnitOfTimeInMinutes { get; set; }
    public int Cost { get; set; }    
    public DateTime CreatedAt { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime StoppedAt { get; init; }
    public DateTime? SynchronizedWithCloudAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedBy { get; set; }
    public bool IsUploadedToCloud { get; set; }
}