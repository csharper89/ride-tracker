using CommunityToolkit.Mvvm.ComponentModel;
using Plugin.Maui.Audio;
using RideTracker.Infrastructure.DbModels;
using RideTracker.Resources.Languages;

namespace RideTracker.Vehicles.VehicleList;

public partial class VehicleModel : ObservableObject
{
    [ObservableProperty]
    private string _elapsedTime;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _quickSaveButton1;

    [ObservableProperty]
    private string _quickSaveButton2;

    [ObservableProperty]
    private bool _canDoQuickSave;

    private bool _notifiedAboutTimeIsUp;

    private DateTime EstimatedRideEndTimeUtc => RideStartedAtUtc!.Value.AddMinutes(EstimatedRideTimeInMinutes!.Value);

    public DateTime? RideStartedAtUtc { get; set; }

    public int? EstimatedRideTimeInMinutes { get; set; }

    public bool TimeIsUp => RideStartedAtUtc.HasValue && DateTime.UtcNow > EstimatedRideEndTimeUtc;

    public bool RideInProgress => RideStartedAtUtc.HasValue;

    public int UnitOfTimeInMinutes { get; set; }

    public Guid Id { get; set; }

    private IAudioPlayer _notificationPlayer;

    public VehicleModel(Vehicle vehicle)
    {
        Id = vehicle.Id;
        Name = vehicle.Name;
        UnitOfTimeInMinutes = vehicle.UnitOfTimeInMinutes;
        CanDoQuickSave = true;

        var quickSaveButtons = vehicle.QuickSaveButtons.Split(',');
        QuickSaveButton1 = quickSaveButtons[0];
        QuickSaveButton2 = quickSaveButtons[1];

        _notificationPlayer = AudioManager.Current.CreatePlayer("RZFWLXE-bell-hop-bell.mp3");
    }

    public void UpdateElapsedTime(DateTime currentTimeUtc)
    {
        if (RideInProgress)
        {
            var elapsedTimeSpan = currentTimeUtc - RideStartedAtUtc.Value;
            ElapsedTime = $"{(int)elapsedTimeSpan.TotalMinutes}:{elapsedTimeSpan.Seconds:D2} / {EstimatedRideTimeInMinutes}";

            if (TimeIsUp && !_notifiedAboutTimeIsUp)
            {
                _notifiedAboutTimeIsUp = true;
                _notificationPlayer.Play();
            }
        }
    }

    public void StartRide(DateTime currentTimeUtc)
    {
        if(RideStartedAtUtc is not null)
        {
            throw new InvalidOperationException("Ride has already started.");
        }

        RideStartedAtUtc = currentTimeUtc;
        EstimatedRideTimeInMinutes = 0;
        CanDoQuickSave = false;
        _notifiedAboutTimeIsUp = false;
    }

    public async Task EndRide(DateTime currentTime)
    {
        if (RideStartedAtUtc is null)
        {
            throw new InvalidOperationException("Ride has not started yet.");
        }
        
        RideStartedAtUtc = null;
        EstimatedRideTimeInMinutes = null;
        ElapsedTime = string.Empty;
        CanDoQuickSave = true;
    }

    public void IncreaseEstimatedRideTime(int minutes)
    {
        if (RideStartedAtUtc is null)
        {
            throw new InvalidOperationException("Ride has not started yet.");
        }

        EstimatedRideTimeInMinutes += minutes;
    }
}
