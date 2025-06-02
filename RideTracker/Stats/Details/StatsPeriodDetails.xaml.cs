namespace RideTracker.Stats.Details;

public partial class StatsPeriodDetailsPage : ContentPage
{
    private readonly StatsPeriodDetailsViewModel _viewModel;

    public StatsPeriodDetailsPage(StatsPeriodDetailsViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();        
        await _viewModel.InitializeAsync();
    }
}