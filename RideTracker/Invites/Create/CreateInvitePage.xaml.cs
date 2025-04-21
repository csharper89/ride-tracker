namespace RideTracker.Invites;

public partial class CreateInvitePage : ContentPage
{
	public CreateInvitePage(CreateInviteViewModel model)
	{
		InitializeComponent();
		BindingContext = model;
	}
}