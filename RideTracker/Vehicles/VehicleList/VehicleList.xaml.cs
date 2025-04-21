namespace RideTracker.Vehicles.VehicleList;

public partial class VehicleListPage : ContentPage
{
    private readonly VehicleListModel _model;

    public VehicleListPage(VehicleListModel model)
    {
        InitializeComponent();
        BindingContext = model;
        _model = model;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _model.Load();        
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }
}