using RideTracker.Infrastructure.DbModels;
using RideTracker.Resources.Languages;

namespace RideTracker.Rides.HistoryForOneDay;

public class RideSummary
{
    public Guid Id { get; set; }
    public string VehicleName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsUploadedToCloud { get; set; }
    public int PricePerUnitOfTime { get; set; }
    public int RideDurationInMinutes { get; set; }
    public int UnitOfTimeInMinutes { get; set; }
    public int Cost { get; set; }

    public string NameAndTime => $"{VehicleName}   {DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc).ToLocalTime().ToString("HH:mm")}";
    public string DetailedCost => $"{RideDurationInMinutes} {AppResources.Minutes} X {PricePerUnitOfTime} = {RideDurationInMinutes / UnitOfTimeInMinutes * PricePerUnitOfTime}{AppResources.RideDetails_Currency}";
    public Color SynchronizationIndicatorColor => IsUploadedToCloud ? Colors.LightGreen : Colors.Orange;
}
