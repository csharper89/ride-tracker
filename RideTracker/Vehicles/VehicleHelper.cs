using RideTracker.Infrastructure.DbModels;
using SQLite;
namespace RideTracker.Vehicles;

public class VehicleHelper(ISQLiteAsyncConnection db)
{
    private Dictionary<Guid, string> _cachedVehicleNames;

    public virtual async ValueTask<string> GetVehicleNameAsync(Guid vehicleId)
    {
        if(_cachedVehicleNames is null)
        {
            await LoadVehicleNamesAsync();
        }

        return _cachedVehicleNames![vehicleId];
    }    

    public virtual async Task LoadVehicleNamesAsync()
    {
        var vehicles = await db.Table<Vehicle>().ToListAsync();
        _cachedVehicleNames = vehicles.ToDictionary(v => v.Id, v => v.Name);
    }
}
