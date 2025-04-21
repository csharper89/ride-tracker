using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Firebase.Auth;
using RideTracker.Resources.Languages;
using System.Net.Mail;
using RideTracker.Authentication.Services;
using RideTracker.Groups.List;
using RideTracker.Infrastructure;

namespace RideTracker.PageModels;

public partial class SignUpModel(DbLogger<SignUpModel> logger, IAuthenticationService authenticationService) : ObservableObject
{
    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _passwordRetyped;

    [ObservableProperty]
    private string _emailError;

    [ObservableProperty]
    private string _passwordError;

    [ObservableProperty]
    private string _signUpError;

    private bool CanSignUp()
    {
        var emailIsValid = ValidateEmail();
        var passwordIsValid = ValidatePassword();

        return emailIsValid && passwordIsValid;
    }

    private bool ValidateEmail()
    {
        EmailError = string.Empty;

        if (string.IsNullOrWhiteSpace(Email))
        {
            EmailError = AppResources.SignUp_EmailIsEmpty;
            return false;
        }

        try
        {
            new MailAddress(Email);
            return true;
        }
        catch (FormatException)
        {
            EmailError = AppResources.SignUp_InvalidEmail;
            return false;
        }
    }

    private bool ValidatePassword()
    {
        PasswordError = string.Empty;

        if (Password != PasswordRetyped)
        {
            PasswordError = AppResources.SignUp_PasswordsDontMatch;
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = AppResources.SignUp_PasswordIsEmpty;
            return false;
        }

        if (Password.Length < 6)
        {
            PasswordError = AppResources.SignUp_WeakPassword;
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task SignUp()
    {
        logger.LogInformation("SignUp operation started.");
        if (!CanSignUp())
        {
            logger.LogInformation("SignUp operation failed validation.");
            return;
        }

        try
        {
            await authenticationService.SignUpAsync(Email, Password);
            logger.LogInformation($"User with email {Email} signed up successfully.");
        }
        catch (FirebaseAuthHttpException ex)
        {
            logger.LogError(ex, $"SignUp operation failed for email {Email}.");
            SignUpError = "Error during sign up: " + ex.Message; // Add appropriate error handling
        }

        Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
        await Shell.Current.GoToAsync("//" + nameof(GroupsListPage));
    }
}