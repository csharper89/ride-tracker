namespace RideTracker.Vehicles.VehicleDetails;

public partial class VehicleDetailsPage : ContentPage
{
    private readonly VehicleDetailsViewModel _viewModel;

    public VehicleDetailsPage(VehicleDetailsViewModel model)
	{
		InitializeComponent();
        BindingContext = model;
        _viewModel = model;
    }

	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _viewModel.InitializeAsync();
    }
}