namespace RideTracker.Rides.Details;

public record RideDetailsDto(Guid VehicleId, DateTime StartedAt, DateTime StoppedAt, int EstimatedRideInMinutes);
