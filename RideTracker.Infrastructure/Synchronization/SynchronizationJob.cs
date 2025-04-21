using FluentScheduler;
using RideTracker.Authentication.Services;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Infrastructure.Synchronization.Logs;
using RideTracker.Infrastructure.Synchronizers;
using RideTracker.Rides.Synchronization;
using RideTracker.Vehicles.Synchronization;
using SQLite;

namespace RideTracker.Infrastructure.Synchronization;

public class SynchronizationJob(GroupsSynchronizer groupsSynchronizer, VehiclesSynchronizer vehiclesSynchronizer, RidesSynchronizer ridesSynchronizer, DbLogger<SynchronizationJob> logger, IAuthenticationService authenticationService, ISQLiteAsyncConnection db, RideTrackerHttpClient httpClient) : IJob
{
    public async void Execute()
    {
        if(!authenticationService.IsSignedIn)
        {
            return;
        }

        try
        {
            await UploadLogsAsync();

            await groupsSynchronizer.FetchEntitiesFromCloudAsync();
            await groupsSynchronizer.UploadEntitiesToCloudAsync();

            await vehiclesSynchronizer.FetchEntitiesFromCloudAsync();
            await vehiclesSynchronizer.UploadEntitiesToCloudAsync();

            await ridesSynchronizer.FetchEntitiesFromCloudAsync();          
            await ridesSynchronizer.UploadEntitiesToCloudAsync();            
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error synchronizing data");
        }
    }

    public async Task UploadLogsAsync()
    {
        var logs = await db.Table<Log>().ToListAsync();
        if(logs.Count == 0)
        {
            return;
        }

        var request = new UploadLogsRequest
        {
            Logs = logs
        };

        var res = await httpClient.PostAsync<SynchronizationTimeResponse>("api/logs", request);
        foreach (var log in logs)
        {            
            await db.DeleteAsync(log);
        }
    }
}
