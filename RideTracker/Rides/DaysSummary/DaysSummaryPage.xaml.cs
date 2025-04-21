namespace RideTracker.Rides.ListOfDays;

public partial class DaysSummaryPage : ContentPage
{
    private readonly DaysSummaryViewModel _viewModel;

    public DaysSummaryPage(DaysSummaryViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSummariesAsync();
    }
}