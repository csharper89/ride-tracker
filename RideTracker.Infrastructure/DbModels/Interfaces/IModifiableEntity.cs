namespace RideTracker.Infrastructure.DbModels.Interfaces;

public interface IModifiableEntity
{
    DateTime? UpdatedAt { get; set; }
}
