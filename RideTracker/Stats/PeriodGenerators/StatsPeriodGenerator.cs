using SQLite;

namespace RideTracker.Stats.PeriodGenerators;

public abstract class StatsPeriodGenerator(ISQLiteAsyncConnection db, TimeProvider timeProvider, GroupUtils groupUtils)
{
    public async Task<List<StatsPeriod>> GetPeriodsAsync()
    {
        var today = timeProvider.GetLocalNow().Date;
        var groupId = await groupUtils.GetCurrentGroupIdAsync();
        var rides = await db.QueryAsync<RideTimeAndCost>(@"SELECT r.CreatedAt, r.Cost
                                                                    FROM Rides r 
                                                                    JOIN Vehicles v ON r.VehicleId = v.Id 
                                                                    WHERE v.GroupId = ?                 
                                                                    AND r.DeletedAt IS NULL", [groupId]);

        var startDate = rides.Min(x => x.CreatedAt);

        return GetPeriods(startDate.Date, today, rides)
            .Where(x => x.TotalCostPerPeriod > 0)
            .OrderByDescending(x => x.Start)
            .ToList();
    }

    protected abstract List<StatsPeriod> GetPeriods(DateTime startDate, DateTime endDate, List<RideTimeAndCost> rides);
}
