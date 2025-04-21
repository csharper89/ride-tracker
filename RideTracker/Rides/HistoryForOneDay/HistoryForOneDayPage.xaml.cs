namespace RideTracker.Rides.History;

public partial class HistoryForOneDayPage : ContentPage
{
    private readonly RideHistoryViewModel _model;

    public HistoryForOneDayPage(RideHistoryViewModel model)
    {
        InitializeComponent();
        _model = model;
        BindingContext = model;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _model.InitializeAsync();
    }
}