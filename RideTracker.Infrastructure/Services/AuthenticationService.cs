using Firebase.Auth;

namespace RideTracker.Authentication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly FirebaseAuthClient _authClient;

    public bool IsSignedIn => _authClient.User != null;

    public AuthenticationService(FirebaseAuthClient authClient)
    {
        _authClient = authClient;
    }

    public async Task SignInAsync(string email, string password)
    {
        await _authClient.SignInWithEmailAndPasswordAsync(email, password);
    }

    public async Task SignUpAsync(string email, string password)
    {
        await _authClient.CreateUserWithEmailAndPasswordAsync(email, password);
    }

    public async Task<string> GetIdTokenAsync()
    {
        return await _authClient.User!.GetIdTokenAsync();
    }

    public string GetCurrentUserEmail()
    {
        return _authClient.User!.Info.Email;
    }
}