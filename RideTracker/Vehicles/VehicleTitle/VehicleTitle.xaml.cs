using Microsoft.Maui;
using System.Windows.Input;

namespace RideTracker.Vehicles.VehicleTitle;

public partial class VehicleTitle : ContentView
{
    public static readonly BindableProperty IdProperty =
        BindableProperty.Create(nameof(Id), typeof(object), typeof(VehicleTitle));

    public object Id
    {
        get => GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    // Bindable property for Name
    public static readonly BindableProperty NameProperty =
        BindableProperty.Create(nameof(Name), typeof(string), typeof(VehicleTitle));

    public string Name
    {
        get => (string)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    // Bindable property for Command
    public static readonly BindableProperty OpenVehicleDetailsCommandProperty =
        BindableProperty.Create(nameof(OpenVehicleDetailsCommand), typeof(ICommand), typeof(VehicleTitle));

    public ICommand OpenVehicleDetailsCommand
    {
        get => (ICommand)GetValue(OpenVehicleDetailsCommandProperty);
        set => SetValue(OpenVehicleDetailsCommandProperty, value);
    }

    public VehicleTitle()
	{
		InitializeComponent();

        double fontSize = 23;
        double minFontSize = 15;
        this.SizeChanged += (s, e) =>
        {
            while (fontSize > minFontSize)
            {
                Title.FontSize = fontSize;
                var size = Title.Measure(double.PositiveInfinity, double.PositiveInfinity);

                if (size.Width <= (Title.Parent as VisualElement).Width)
                    break;

                fontSize -= 1;
            }

            Title.FontSize = fontSize;
        };
    }
}