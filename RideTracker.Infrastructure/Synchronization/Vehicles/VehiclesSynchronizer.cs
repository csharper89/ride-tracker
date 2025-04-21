using RideTracker.Infrastructure.DbModels;
using RideTracker.Infrastructure;
using SQLite;
using RideTracker.Utilities;
using RideTracker.Infrastructure.Synchronization;

namespace RideTracker.Vehicles.Synchronization;

public class VehiclesSynchronizer : EntitySynchronizer<Vehicle, VehicleResponse>
{
    public VehiclesSynchronizer(RideTrackerHttpClient httpClient, ISQLiteAsyncConnection db, DbLogger<EntitySynchronizer<Vehicle, VehicleResponse>> logger, TimeProvider timeProvider, GroupUtils groupUtils) : base(httpClient, db, logger, timeProvider, groupUtils)
    {
    }

    protected override object GetUploadRequest(Vehicle vehicle)
    {
        return new CreateOrUpdateVehicleRequest
        {
            VehicleId = vehicle.Id,
            Name = vehicle.Name,
            PricePerUnitOfTime = vehicle.PricePerUnitOfTime,
            UnitOfTimeInMinutes = vehicle.UnitOfTimeInMinutes,
            CreatedAt = vehicle.CreatedAt,
            UpdatedAt = vehicle.UpdatedAt,
            DeletedAt = vehicle.DeletedAt,
            QuickSaveButtons = vehicle.QuickSaveButtons,
            GroupId = vehicle.GroupId
        };
    }

    protected override Vehicle CreateEntityFromResponse(VehicleResponse vehicleResponse)
    {
        return new Vehicle
        {
            Id = vehicleResponse.Id,
            Name = vehicleResponse.Name,
            PricePerUnitOfTime = vehicleResponse.PricePerUnitOfTime,
            UnitOfTimeInMinutes = vehicleResponse.UnitOfTimeInMinutes,
            UpdatedAt = vehicleResponse.UpdatedAt,
            GroupId = vehicleResponse.GroupId,
            DeletedAt = vehicleResponse.DeletedAt,
            CreatedAt = vehicleResponse.CreatedAt,
            QuickSaveButtons = vehicleResponse.QuickSaveButtons,
            IsUploadedToCloud = true,
            SynchronizedWithCloudAt = vehicleResponse.SynchronizedWithCloudAt
        };
    }

    protected override void UpdateEntityFromResponse(VehicleResponse vehicleResponse, Vehicle existingVehicle)
    {
        existingVehicle.Name = vehicleResponse.Name;
        existingVehicle.PricePerUnitOfTime = vehicleResponse.PricePerUnitOfTime;
        existingVehicle.UnitOfTimeInMinutes = vehicleResponse.UnitOfTimeInMinutes;
        existingVehicle.UpdatedAt = vehicleResponse.UpdatedAt;
        existingVehicle.QuickSaveButtons = vehicleResponse.QuickSaveButtons;
        existingVehicle.DeletedAt = vehicleResponse.DeletedAt;
        existingVehicle.SynchronizedWithCloudAt = vehicleResponse.SynchronizedWithCloudAt;
    }

    protected override async Task<string> GetFetchEndpointAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return $"api/groups/{groupId}/vehicles";
    }

    protected override async Task<string> GetUploadEndpointAsync(Vehicle vehicle)
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return $"api/groups/{groupId}/vehicles";
    }

    protected override async Task<bool> CanUploadAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return groupId.HasValue;
    }

    protected override async Task<bool> CanFetchAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return groupId.HasValue;
    }

    protected async override Task<DateTime> GetLastSynchronizationTimeAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return await _db.ExecuteScalarAsync<DateTime?>($"SELECT MAX(SynchronizedWithCloudAt) FROM Vehicles WHERE GroupId = '{groupId}'") ?? DateTime.MinValue;
    }
}
