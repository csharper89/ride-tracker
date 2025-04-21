namespace RideTracker.Authentication.Services;

public interface IAuthenticationService
{
    Task SignInAsync(string email, string password);
    Task SignUpAsync(string email, string password);
    Task<string> GetIdTokenAsync();
    string GetCurrentUserEmail();
    bool IsSignedIn { get; }
}