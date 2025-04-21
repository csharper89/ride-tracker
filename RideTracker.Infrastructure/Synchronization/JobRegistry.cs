using FluentScheduler;

namespace RideTracker.Infrastructure.Synchronization;

public class JobRegistry : Registry
{
    public JobRegistry(SynchronizationJob synchronizationJob)
    {
        Schedule(synchronizationJob).NonReentrant().ToRunNow().AndEvery(5).Minutes();
    }
}
