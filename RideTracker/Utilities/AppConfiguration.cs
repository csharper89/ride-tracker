using RideTracker.Infrastructure.DbModels;
using SQLite;

namespace RideTracker.Utilities;

public class AppConfiguration(SQLiteConnection db)
{
    private int? _rideTimeIncrementMinutes;

    public virtual int RideTimeIncrementMinutes
    {
        get
        {
            return 5;
            if (_rideTimeIncrementMinutes is null)
            {
                var value = db.Table<Setting>().First(x => x.Key == "RideTimeIncrementMinutes").Value;
                _rideTimeIncrementMinutes = int.Parse(value);
            }

            return _rideTimeIncrementMinutes.Value;
        }
    }
}
