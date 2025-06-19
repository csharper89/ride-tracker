using RideTracker.Authentication.Services;
using RideTracker.Infrastructure.DbModels;
using SQLite;

namespace RideTracker.Rides.HistoryForOneDay;

public class SalaryCalculatorService
{
    public const int BenefitsStartAt = 4000;
    private readonly ISQLiteAsyncConnection _db;
    private readonly IAuthenticationService _authService;
    private readonly GroupUtils _groupUtils;

    public SalaryCalculatorService(ISQLiteAsyncConnection db, IAuthenticationService authService, GroupUtils groupUtils)
    {
        _db = db;
        _authService = authService;
        _groupUtils = groupUtils;
    }

    public int CalculateSalary(int sum, DateTime date)
    {
        int baseSalary = GetBaseSalary(date);

        if (sum <= baseSalary)
        {
            return sum;
        }

        if (sum < BenefitsStartAt)
        {
            return baseSalary;
        }

        var benefits = 0;
        var currentLevel = BenefitsStartAt;

        while (currentLevel <= sum)
        {
            benefits += 100;
            currentLevel += 1000;
        }

        return baseSalary + benefits;
    }

    private bool IsWeekend(DateTime date)
    {
        var dayOfWeek = date.DayOfWeek;
        return dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday;
    }

    public int GetBaseSalary(DateTime date)
    {
        return IsWeekend(date) ? 1400 : 800;
    }

    public async Task<List<DateAndSum>> GetDatesWithRidesByOtherUsersAsync(DateTime start, DateTime end)
    {
        var currentUser = _authService.GetCurrentUserEmail();
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        if (groupId == null)
        {
            return new List<DateAndSum>();
        }

        var rides = await _db.QueryAsync<Ride>(
            @"SELECT r.CreatedAt, r.Cost FROM Rides r
              INNER JOIN Vehicles v ON r.VehicleId = v.Id
              INNER JOIN Groups g ON v.GroupId = g.Id
              WHERE r.CreatedAt >= ? AND r.CreatedAt <= ? AND r.DeletedAt IS NULL AND r.CreatedBy != ? AND g.Id = ?",
            start.Date, new DateTime(end.Year, end.Month, end.Day, 23, 59, 59), currentUser, groupId.Value
        );

        return rides
            .GroupBy(r => r.CreatedAt.Date)
            .Select(g => new DateAndSum { Date = g.Key, TotalSum = g.Sum(r => r.Cost) })
            .ToList();
    }

    public async Task<int> CalculateSalaryForPeriodAsync(DateTime start, DateTime end)
    {
        var days = await GetDatesWithRidesByOtherUsersAsync(start, end);
        return days.Select(d => new
        {
            Salary = CalculateSalary(d.TotalSum, d.Date)
        })
           .Sum(x => x.Salary);
    }
}