namespace RideTracker.Vehicles;

public class VehicleDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required decimal PricePerUnitOfTime { get; set; }
    public DateTime? RideStartedAtUtc { get; set; }
    public int? EstimatedRideTimeInMinutes { get; set; }
}
