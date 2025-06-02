namespace RideTracker.Stats;

public class StatsPeriod
{
    public required DateTime Start { get; set; }
    public required DateTime End { get; set; }
    public required string Title { get; set; }
    public required int TotalCostPerPeriod { get; set; }
}
