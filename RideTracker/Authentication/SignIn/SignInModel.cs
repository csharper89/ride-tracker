using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using RideTracker.Authentication.Services;
using RideTracker.Groups.List;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.Synchronizers;
using RideTracker.Resources.Languages;

namespace RideTracker.PageModels;

public partial class SignInModel(DbLogger<SignInModel> logger, IAuthenticationService authenticationService, GroupsSynchronizer groupsSynchronizer) : ObservableObject
{
    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _errorMessage;

    [ObservableProperty]
    private bool _errorMessageVisible;

    [RelayCommand]
    private async Task SignIn()
    {
        ErrorMessageVisible = false;
        logger.LogInformation("SignIn operation started.");
        try
        {
            await authenticationService.SignInAsync(Email, Password);
            logger.LogInformation($"User with email {Email} signed in successfully.");
            Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
            await groupsSynchronizer.FetchEntitiesFromCloudAsync();
            await Shell.Current.GoToAsync("//" + nameof(GroupsListPage));
        }
        catch (Exception ex)
        {
            ErrorMessage = AppResources.SignIn_FailedToLogin;
            ErrorMessageVisible = true;
            logger.LogError(ex, $"SignIn operation failed for email {Email}.");
        }
    }

    [RelayCommand]
    private async Task SignUp()
    {
        logger.LogInformation("Navigating to SignUp page.");
        await Shell.Current.GoToAsync(nameof(SignUpPage));
    }
}