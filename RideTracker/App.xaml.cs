using FluentScheduler;
using RideTracker.Authentication.Services;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.Database;
using RideTracker.Infrastructure.Synchronization;
using RideTracker.Rides.DaysSummary;

namespace RideTracker
{
    public partial class App : Application
    {
        private readonly IAuthenticationService _authService;
        private readonly JobRegistry _jobRegistry;
        private readonly DbLogger<App> _logger;

        public App(DatabaseMigrator databaseMigrator, RideHistoryHelper rideHistoryHelper, IAuthenticationService authService, JobRegistry jobRegistry, DbLogger<App> logger)
        {
            InitializeComponent();
            Task.Run(databaseMigrator.MigrateAsync).Wait();
            rideHistoryHelper.UpdateSummariesAsync();
            _authService = authService;

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            _jobRegistry = jobRegistry;
            _logger = logger;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex);
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException(e.Exception);
            e.SetObserved(); // Prevents the app from crashing due to unobserved exceptions
        }

        private void LogException(Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
        }

        protected override async void OnStart()
        {
            base.OnStart();
            JobManager.Initialize(_jobRegistry);
            if (!_authService.IsSignedIn)
            {
                Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
                await Shell.Current.GoToAsync(nameof(SignInPage));
            }
#if ANDROID
            new BatteryOptimizationService().RequestIgnoreBatteryOptimizations();
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}