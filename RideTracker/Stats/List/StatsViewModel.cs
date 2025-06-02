using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RideTracker.Stats.Details;
using RideTracker.Stats.PeriodGenerators;

namespace RideTracker.Stats.List;

public partial class StatsViewModel : ObservableObject
{
    [ObservableProperty]
    private List<StatsPeriod> _periods;

    public async Task InitializeAsync(StatsPeriodGenerator periodGenerator)
    {
        Periods = await periodGenerator.GetPeriodsAsync();
    }

    [RelayCommand]
    private async Task OpenPeriodAsync(StatsPeriod period)
    {        
        await Shell.Current.GoToAsync(nameof(StatsPeriodDetailsPage), new Dictionary<string, object>
        {
            { "Period", period }
        });        
    }
}
