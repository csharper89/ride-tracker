namespace RideTracker.Vehicles.Synchronization;

public class CreateOrUpdateVehicleRequest
{
    public Guid VehicleId { get; init; }
    public string Name { get; init; }
    public int PricePerUnitOfTime { get; init; }
    public int UnitOfTimeInMinutes { get; init; }
    public string QuickSaveButtons { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? DeletedAt { get; init; }
    public Guid GroupId { get; init; }
}
