using CommunityToolkit.Mvvm.ComponentModel;
using RideTracker.Rides.HistoryForOneDay;
using SQLite;

namespace RideTracker.Stats.Details;

[QueryProperty(nameof(Period), nameof(Period))]
public partial class StatsPeriodDetailsViewModel(ISQLiteAsyncConnection db, GroupUtils groupUtils, SalaryCalculatorService salaryCalculatorService) : ObservableObject
{
    [ObservableProperty]
    private List<CarStats> _carStats;

    [ObservableProperty]
    private StatsPeriod _period;

    [ObservableProperty]
    private int _total;

    [ObservableProperty]
    private int _salaryForPeriod;

    public async Task InitializeAsync()
    {
        var currentGroupId = await groupUtils.GetCurrentGroupIdAsync();
        var stats = await db.QueryAsync<CarStats>(@"SELECT v.Name, SUM(r.Cost) as TotalCost, count(*) as RidesCount
                                                    FROM Rides r 
                                                    JOIN Vehicles v ON r.VehicleId = v.Id 
                                                    WHERE v.GroupId = ? 
                                                    AND r.CreatedAt > ?
                                                    AND r.CreatedAt < ?
                                                    AND r.DeletedAt IS NULL 
                                                    GROUP BY v.Name", [currentGroupId, Period.Start, GetEndOfDay(Period.End)]);

        CarStats = stats.OrderBy(x => x.TotalCost).ToList();
        Total = CarStats.Sum(x => x.TotalCost);
        SalaryForPeriod = await salaryCalculatorService.CalculateSalaryForPeriodAsync(Period.Start, Period.End);
    }

    public static DateTime GetEndOfDay(DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59);
    }
}
