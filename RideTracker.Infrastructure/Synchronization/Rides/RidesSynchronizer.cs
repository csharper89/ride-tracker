using RideTracker.Infrastructure.DbModels;
using RideTracker.Infrastructure;
using SQLite;
using RideTracker.Utilities;
using RideTracker.Infrastructure.Synchronization;
using RideTracker.Rides.DaysSummary;
using Newtonsoft.Json;

namespace RideTracker.Rides.Synchronization;

public class RidesSynchronizer : EntitySynchronizer<Ride, RideResponse>
{
    private readonly RideHistoryHelper _rideHistoryHelper;
    private readonly DbLogger<EntitySynchronizer<Ride, RideResponse>> _logger;

    public RidesSynchronizer(RideTrackerHttpClient httpClient, ISQLiteAsyncConnection db, DbLogger<EntitySynchronizer<Ride, RideResponse>> logger, TimeProvider timeProvider, GroupUtils groupUtils, RideHistoryHelper rideHistoryHelper) : base(httpClient, db, logger, timeProvider, groupUtils)
    {
        _rideHistoryHelper = rideHistoryHelper;
        _logger = logger;
    }

    protected override object GetUploadRequest(Ride ride)
    {
        return new CreateOrUpdateRideRequest
        {
            RideId = ride.Id,
            VehicleId = ride.VehicleId,
            VehicleName = ride.VehicleName,
            RideDurationInMinutes = ride.RideDurationInMinutes,
            UnitOfTimeInMinutes = ride.UnitOfTimeInMinutes,
            PricePerUnitOfTime = ride.PricePerUnitOfTime,
            Cost = ride.Cost,
            CreatedAt = ride.CreatedAt,
            StartedAt = ride.StartedAt,
            StoppedAt = ride.StoppedAt,
            UpdatedAt = ride.UpdatedAt,
            DeletedAt = ride.DeletedAt,
            CreatedBy = ride.CreatedBy
        };
    }

    protected override Ride CreateEntityFromResponse(RideResponse rideResponse)
    {
        _logger.LogInformation($"CreateEntityFromResponse: {JsonConvert.SerializeObject(rideResponse)}");
        return new Ride
        {
            Id = rideResponse.Id,
            VehicleId = rideResponse.VehicleId,
            VehicleName = rideResponse.VehicleName,
            RideDurationInMinutes = rideResponse.RideDurationInMinutes,
            PricePerUnitOfTime = rideResponse.PricePerUnitOfTime,
            UnitOfTimeInMinutes = rideResponse.UnitOfTimeInMinutes,
            DeletedAt = rideResponse.DeletedAt,
            Cost = rideResponse.Cost,
            CreatedAt = rideResponse.CreatedAt,
            StartedAt = rideResponse.StartedAt,
            StoppedAt = rideResponse.StoppedAt,
            SynchronizedWithCloudAt = rideResponse.SynchronizedWithCloudAt,
            IsUploadedToCloud = true,
            CreatedBy = rideResponse.CreatedBy
        };
    }

    protected override void UpdateEntityFromResponse(RideResponse rideResponse, Ride existingRide)
    {
        _logger.LogInformation($"UpdateEntityFromResponse: {JsonConvert.SerializeObject(rideResponse)}");

        existingRide.VehicleId = rideResponse.VehicleId;
        existingRide.VehicleName = rideResponse.VehicleName;
        existingRide.RideDurationInMinutes = rideResponse.RideDurationInMinutes;
        existingRide.PricePerUnitOfTime = rideResponse.PricePerUnitOfTime;
        existingRide.UnitOfTimeInMinutes = rideResponse.UnitOfTimeInMinutes;
        existingRide.Cost = rideResponse.Cost;
        existingRide.UpdatedAt = rideResponse.UpdatedAt;
        existingRide.DeletedAt = rideResponse.DeletedAt;
        existingRide.SynchronizedWithCloudAt = rideResponse.SynchronizedWithCloudAt;
        existingRide.CreatedBy = rideResponse.CreatedBy;
    }

    protected override async Task<string> GetFetchEndpointAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return $"api/groups/{groupId}/rides";
    }

    protected override Task<string> GetUploadEndpointAsync(Ride ride)
    {
        return Task.FromResult($"api/vehicles/{ride.VehicleId}/rides");
    }

    protected override async Task<bool> CanUploadAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return groupId.HasValue;
    }

    protected override async Task<bool> CanFetchAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        var userIsManagingCurrentGroup = await _groupUtils.IsUserManagingCurrentGroupAsync();
        return groupId.HasValue && userIsManagingCurrentGroup;
    }

    protected override void OnFetchCompleted(int fetchedCount)
    {
        if (fetchedCount > 0) 
        {
            _rideHistoryHelper.UpdateSummariesAsync();
        }
    }

    protected async override Task<DateTime> GetLastSynchronizationTimeAsync()
    {
        var groupId = await _groupUtils.GetCurrentGroupIdAsync();
        return await _db.ExecuteScalarAsync<DateTime?>($@"SELECT MAX(r.SynchronizedWithCloudAt) FROM Rides r 
                                                            JOIN Vehicles v ON r.VehicleId = v.Id
                                                            WHERE v.GroupId = '{groupId}'") ?? DateTime.MinValue;
    }
}
