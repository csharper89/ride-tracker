using RideTracker.Resources.Languages;

namespace RideTracker.Utilities;

public class AlertsService
{
    private Page MainPage => Application.Current.MainPage;

    public async virtual Task<bool> ConfirmAsync(string title, string message)
    {
        return await MainPage.DisplayAlert(title, message, AppResources.Alert_Yes, AppResources.Alert_No);
    }
}
