using RideTracker.Utilities;
using SQLite;

namespace RideTracker.Rides.DaysSummary;

public class RideHistoryHelper(SQLiteAsyncConnection db, TimeProvider timeProvider, GroupUtils groupUtils)
{
    private DateTime? _date;
    private List<SummaryForDay>? _dates;

    public virtual async ValueTask<List<SummaryForDay>> GetDatesSummaryAsync()
    {
        var localNow = timeProvider.GetLocalNow().Date;
        if (_date != localNow)
        {
            await InitializeAsync();
        }

        return _dates!;
    }

    public virtual async Task UpdateSummariesAsync()
    {
        _date = null;
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            //_date = timeProvider.GetLocalNow().Date;
            _dates = await GetDatesSummaryFromDatabaseAsync();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task<List<SummaryForDay>> GetDatesSummaryFromDatabaseAsync()
    {
        var daysSummary = await FetchSummaryAsync();
        AddMissingDays(daysSummary);
        PopulateStringValuesForDates(daysSummary);

        return daysSummary
            .OrderByDescending(s => s.Date)
            .ToList();
    }

    private void PopulateStringValuesForDates(List<SummaryForDay> daysSummary)
    {
        foreach (var summary in daysSummary)
        {
            summary.DateString = summary.Date.ToString("dd.MM.yyyy");
        }
    }

    private void AddMissingDays(List<SummaryForDay> daysSummary)
    {
        var hashSet = daysSummary
            .Select(s => s.Date)
            .ToHashSet();

        var today = timeProvider.GetLocalNow().Date;
        if (!hashSet.Contains(today))
        {
            daysSummary.Add(new SummaryForDay { Date = today, TotalSumForDay = 0 });
            hashSet.Add(today);
        }

        var firstDay = daysSummary.Min(s => s.Date);
        var currentDate = firstDay;
        while (currentDate < today)
        {
            currentDate = currentDate.AddDays(1);
            if (!hashSet.Contains(currentDate))
            {
                daysSummary.Add(new SummaryForDay { Date = currentDate, TotalSumForDay = 0 });
            }
        }
    }

    private async Task<List<SummaryForDay>> FetchSummaryAsync()
    {
        var today = timeProvider.GetLocalNow();
        var beginningOfYear = new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Local).ToUniversalTime();
        var currentGroupId = await groupUtils.GetCurrentGroupIdAsync();
        var parameters = new object[] { currentGroupId, beginningOfYear.Ticks };
        var allRidesForCurrentYear = await db.QueryAsync<RideTimeAndCost>(@"SELECT r.CreatedAt as Time, Cost from Rides r 
                                join Vehicles v on v.Id = r.VehicleId 
                                join Groups g on g.Id = v.GroupId 
                                where r.DeletedAt IS NULL AND g.Id = ? AND r.CreatedAt >= ?", parameters);

        allRidesForCurrentYear.ForEach(r => r.Time = DateTime.SpecifyKind(r.Time, DateTimeKind.Utc).ToLocalTime());

        return allRidesForCurrentYear
            .GroupBy(r => r.Time.Date)
            .Select(g => new SummaryForDay
            {
                Date = g.Key,
                TotalSumForDay = g.Sum(r => r.Cost)
            })
            .ToList();
    }
}
