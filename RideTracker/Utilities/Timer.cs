using System.Diagnostics;

namespace RideTracker.Utilities;

public class Timer : ITimer
{
    private List<Action> _actions = new List<Action>();
    private System.Timers.Timer _timer;

    public Timer()
    {        
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += async (sender, e) =>
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                foreach (var action in _actions)
                {
                    action();
                }
            });
        };
        _timer.Start();
    }

    public void ExecuteActionEverySecond(Action action)
    {
        _actions.Add(action);
    }
}
