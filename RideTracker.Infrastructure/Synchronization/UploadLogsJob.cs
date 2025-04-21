using FluentScheduler;
using RideTracker.Authentication.Services;
using RideTracker.Infrastructure.DbModels;
using SQLite;
using System.Net.Http;

namespace RideTracker.Infrastructure.Synchronization;

public class UploadLogsJob(ISQLiteAsyncConnection db, IAuthenticationService authenticationService, RideTrackerHttpClient httpClient) : IJob
{
    public async void Execute()
    {
        if(!authenticationService.IsSignedIn)
        {
            return;
        }

        
    }
}
