namespace RideTracker.Pages;

public partial class SignUpPage : ContentPage
{
	public SignUpPage(SignUpModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}

    private void Button_Pressed(object sender, EventArgs e)
    {

    }
}