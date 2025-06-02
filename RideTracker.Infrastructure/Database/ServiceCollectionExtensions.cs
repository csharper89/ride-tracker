using Microsoft.Extensions.Http.Resilience;
using Polly;
using RideTracker.Infrastructure.Constants;
using SQLite;

namespace RideTracker.Infrastructure.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<ISQLiteAsyncConnection>(new SQLiteAsyncConnection(Paths.PathToDatabase));
        services.AddSingleton(new SQLiteAsyncConnection(Paths.PathToDatabase));
        services.AddSingleton(new SQLiteConnection(Paths.PathToDatabase));
        services.AddTransient<DatabaseInitializer>();
        services.AddTransient<DatabaseMigrator>();

        return services;
    }

    public static IServiceCollection AddHttpClientWithRetry(this IServiceCollection services)
    {
        services.AddHttpClient("ride-tracker", client =>
        {
            client.BaseAddress = new Uri("https://ride-tracker-d4bddpe4ftcza9a5.westeurope-01.azurewebsites.net/");
        })
            .ConfigurePrimaryHttpMessageHandler(() =>
        {

#if DEBUG
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
#else

            return new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(1),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            };
#endif
        })
            .AddResilienceHandler("custom-retry-policy", (builder, context) =>
        {
            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromSeconds(15),
                ShouldHandle = async args =>
                    args.Outcome.Exception != null || 
                    (args.Outcome.Result is HttpResponseMessage response && !response.IsSuccessStatusCode), 
                OnRetry = args =>
                {
                    var logger = context.ServiceProvider.GetRequiredService<DbLogger<HttpRetryStrategyOptions>>();
                    var response = args.Outcome.Result;
                    var exceptionMessage = args.Outcome.Exception is not null ? $"Exception: {args.Outcome.Exception}" : string.Empty;
                    var responseCodeMessage = response is not null ? $"Response code: {response.StatusCode}." : string.Empty;
                    logger.LogWarning($"Retry {args.AttemptNumber} in {args.RetryDelay.TotalSeconds}s due to failure. SocketsHttpHandler. {responseCodeMessage} {exceptionMessage}");
                    return default;
                }
            });
        });

        return services;
    }
}