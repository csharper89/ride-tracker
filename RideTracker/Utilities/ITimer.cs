namespace RideTracker.Utilities;

public interface ITimer
{
    void ExecuteActionEverySecond(Action action);
}
