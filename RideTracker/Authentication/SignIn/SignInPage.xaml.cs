namespace RideTracker.Pages;

public partial class SignInPage : ContentPage
{
	public SignInPage(SignInModel model)
	{
		InitializeComponent();

		BindingContext = model;
	}
}