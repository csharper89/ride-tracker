using RideTracker.Stats.List;
using RideTracker.Stats.PeriodGenerators;
namespace RideTracker.Stats;

public partial class Stats : ContentPage
{
    private readonly StatsViewModel _model;
    private readonly WeeklyPeriodGenerator _weeklyPeriodGenerator;
    private readonly MonthlyPeriodGenerator _monthlyPeriodGenerator;
    private readonly YearlyPeriodGenerator _yearlyPeriodGenerator;

    public Stats(StatsViewModel model, WeeklyPeriodGenerator weeklyPeriodGenerator, MonthlyPeriodGenerator monthlyPeriodGenerator, YearlyPeriodGenerator yearlyPeriodGenerator)
    {
        InitializeComponent();
        BindingContext = model;

        _model = model;
        _weeklyPeriodGenerator = weeklyPeriodGenerator;
        _monthlyPeriodGenerator = monthlyPeriodGenerator;
        _yearlyPeriodGenerator = yearlyPeriodGenerator;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (_model.Periods is not null && _model.Periods.Count > 0)
        {
            return; // Already initialized
        }

        var route = Shell.Current.CurrentState?.Location?.OriginalString!;
        if(route.EndsWith("StatsWeek"))
        {
            await _model.InitializeAsync(_weeklyPeriodGenerator);
        } 
        else if (route.EndsWith("StatsMonth"))
        {
            await _model.InitializeAsync(_monthlyPeriodGenerator);
        } 
        else
        {
            await _model.InitializeAsync(_yearlyPeriodGenerator);
        }
    }
}