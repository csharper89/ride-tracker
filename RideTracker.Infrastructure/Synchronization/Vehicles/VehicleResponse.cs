using RideTracker.Infrastructure;

namespace RideTracker.Vehicles.Synchronization;

public class VehicleResponse : ApiResponseBase
{
    public string Name { get; init; }
    public int PricePerUnitOfTime { get; init; }
    public int UnitOfTimeInMinutes { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? SynchronizedWithCloudAt { get; init; }
    public string QuickSaveButtons { get; init; }
    public DateTime? DeletedAt { get; init; }
    public Guid GroupId { get; init; }
    public DateTime CreatedAt { get; init; }
}
