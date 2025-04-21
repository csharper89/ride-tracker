using RideTracker.Infrastructure.DbModels;

namespace RideTracker.Infrastructure.Synchronization.Logs;

public class UploadLogsRequest
{
    public List<Log> Logs { get; init; }
}
