using RideTracker.Utilities;
using SQLite;

namespace RideTracker.Stats.PeriodGenerators;

public class YearlyPeriodGenerator(ISQLiteAsyncConnection db, TimeProvider timeProvider, GroupUtils groupUtils) : StatsPeriodGenerator(db, timeProvider, groupUtils)
{
    protected override List<StatsPeriod> GetPeriods(DateTime startDate, DateTime endDate, List<RideTimeAndCost> rides)
    {
        var result = new List<StatsPeriod>();

        // Adjust start date to the first day of its year
        startDate = new DateTime(startDate.Year, 1, 1);

        // Adjust end date to the last day of its year
        endDate = new DateTime(endDate.Year, 12, 31);

        // Iterate through the years between startDate and endDate
        var currentStart = startDate;
        while (currentStart <= endDate)
        {
            // Calculate the current year's end
            var currentEnd = new DateTime(currentStart.Year, 12, 31);

            // Create the title as the year (e.g., "2023")
            string title = currentStart.Year.ToString();

            // Add the current year to the result
            var statsPeriod = new StatsPeriod
            {
                Start = currentStart,
                End = currentEnd,
                Title = title,
                TotalCostPerPeriod = rides
                    .Where(r => r.CreatedAt >= currentStart && r.CreatedAt <= currentEnd)
                    .Sum(r => r.Cost),
            };

            result.Add(statsPeriod);

            // Move to the next year
            currentStart = currentStart.AddYears(1);
        }

        return result;
    }
}
