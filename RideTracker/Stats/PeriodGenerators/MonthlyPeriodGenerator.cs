using RideTracker.Stats.PeriodGenerators;
using SQLite;
using System;

namespace RideTracker.Stats;

public class MonthlyPeriodGenerator(ISQLiteAsyncConnection db, TimeProvider timeProvider, GroupUtils groupUtils) : StatsPeriodGenerator(db, timeProvider, groupUtils)
{
    protected override List<StatsPeriod> GetPeriods(DateTime startDate, DateTime endDate, List<RideTimeAndCost> rides)
    {
        var result = new List<StatsPeriod>();

        // Adjust start date to the first day of the month
        startDate = new DateTime(startDate.Year, startDate.Month, 1);

        // Adjust end date to the last day of the month
        endDate = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));

        // Iterate through the months between the start and end dates
        var currentStart = startDate;
        while (currentStart <= endDate)
        {
            // Calculate the end of the current month
            var currentEnd = new DateTime(currentStart.Year, currentStart.Month, DateTime.DaysInMonth(currentStart.Year, currentStart.Month));

            // Create the title (e.g., "March")
            string title = $"{currentStart:MMMM} {currentStart:yy}";

            // Add the current month to the result
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

            // Move to the next month
            currentStart = currentStart.AddMonths(1);
        }

        return result;
    }
}
