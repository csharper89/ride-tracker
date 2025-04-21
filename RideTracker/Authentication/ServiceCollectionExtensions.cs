using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using RideTracker.Authentication.Services;

namespace RideTracker.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services) 
    {
        var config = new FirebaseAuthConfig
        {
            ApiKey = "AIzaSyDFfIB2yFvMT-R-qT1_d6L-MBVQ84zIemI",
            AuthDomain = "maui-test-5cc27.firebaseapp.com",
            Providers = [new EmailProvider()],
            UserRepository = new FileUserRepository("RideTracker")
        };

        services.AddSingleton(new FirebaseAuthClient(config));
        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        services.AddSingleton<SignInPage>();
        services.AddSingleton<SignInModel>();
        services.AddSingleton<SignUpPage>();
        services.AddSingleton<SignUpModel>();

        return services;
    }
}
