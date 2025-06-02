using RideTracker.Utilities;
using SQLite;

namespace RideTracker.Stats.PeriodGenerators;

public class WeeklyPeriodGenerator(ISQLiteAsyncConnection db, TimeProvider timeProvider, GroupUtils groupUtils) : StatsPeriodGenerator(db, timeProvider, groupUtils)
{
    protected override List<StatsPeriod> GetPeriods(DateTime startDate, DateTime endDate, List<RideTimeAndCost> rides)
    {
        var result = new List<StatsPeriod>();

        // Adjust start date to the closest earlier Monday
        if (startDate.DayOfWeek != DayOfWeek.Monday)
        {
            int daysToSubtract = ((int)startDate.DayOfWeek + 7 - (int)DayOfWeek.Monday) % 7;
            startDate = startDate.AddDays(-daysToSubtract);
        }

        // Adjust end date to the closest later Sunday
        if (endDate.DayOfWeek != DayOfWeek.Sunday)
        {
            int daysToAdd = (7 - (int)endDate.DayOfWeek) % 7;
            endDate = endDate.AddDays(daysToAdd);
        }

        // Start iterating from the adjusted start date and add weeks
        var currentStart = startDate;
        while (currentStart <= endDate)
        {
            var currentEnd = currentStart.AddDays(6); // End of the week (6 days later)
            string title = $"{currentStart:dd.MM} - {currentEnd:dd.MM} ({currentEnd:yy})";

            var statsPeriod = new StatsPeriod
            {
                Start = currentStart,
                End = currentEnd,
                Title = title,
                TotalCostPerPeriod = rides
                    .Where(r => r.CreatedAt.Date >= currentStart && r.CreatedAt.Date <= currentEnd)
                    .Sum(r => r.Cost),
            };

            result.Add(statsPeriod);

            // Move to the next week
            currentStart = currentStart.AddDays(7);
        }

        return result;
    }
}
