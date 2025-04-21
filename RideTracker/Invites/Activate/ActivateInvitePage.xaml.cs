namespace RideTracker.Invites.Activate;

public partial class ActivateInvitePage : ContentPage
{
	public ActivateInvitePage(ActivateInviteViewModel model)
	{
		InitializeComponent();
        BindingContext = model;
    }
}