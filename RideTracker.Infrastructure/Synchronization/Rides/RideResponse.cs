using RideTracker.Infrastructure;

namespace RideTracker.Rides.Synchronization;

public class RideResponse : ApiResponseBase
{
    public Guid VehicleId { get; init; }
    public string VehicleName { get; init; }
    public int RideDurationInMinutes { get; init; }
    public int PricePerUnitOfTime { get; init; }
    public int UnitOfTimeInMinutes { get; init; }
    public int Cost { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime StartedAt { get; init; }
    public DateTime StoppedAt { get; init; }
    public DateTime SynchronizedWithCloudAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
    public string CreatedBy { get; init; }
}
