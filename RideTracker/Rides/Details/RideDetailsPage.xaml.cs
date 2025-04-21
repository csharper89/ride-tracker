using RideTracker.Resources.Languages;

namespace RideTracker.Rides.Details;

public partial class RideDetailsPage : ContentPage
{
	private RideDetailsViewModel _model;
    private AlertsService _alertsService;

    public RideDetailsDto RideDetails { get; set; }

    public RideDetailsPage(RideDetailsViewModel model, AlertsService alertsService)
    {
        InitializeComponent();
        _model = model;
        BindingContext = model;
        _alertsService = alertsService;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _model.InitializeAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        Dispatcher.Dispatch(async () =>
        {
            var quitWithoutSaving = await _alertsService.ConfirmAsync(AppResources.Warning, AppResources.RideDetails_CancellationConfirmation);
            if(quitWithoutSaving)
            {
                await Shell.Current.GoToAsync("..");
            }
        });
        
        return true;
    }
}