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
        var a = lbl.FontSize;
        FitTextToLabel(lbl, lbl.Text);
        await _model.Load();        
    }

    private void FitTextToLabel(Label label, string text)
    {
        double fontSize = 30;
        double minFontSize = 10;

        label.Text = text;

        label.SizeChanged += (s, e) =>
        {
            while (fontSize > minFontSize)
            {
                label.FontSize = fontSize;

                var size = label.Measure(label.Width, double.PositiveInfinity);

                if (size.Width <= label.Width)
                    break;

                fontSize -= 1;
            }

            label.FontSize = fontSize;
        };
    }

}