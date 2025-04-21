using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RideTracker.Authentication;
using RideTracker.Groups.Details;
using RideTracker.Groups.List;
using RideTracker.Infrastructure;
using RideTracker.Infrastructure.Database;
using RideTracker.Infrastructure.Synchronization;
using RideTracker.Infrastructure.Synchronizers;
using RideTracker.Invites;
using RideTracker.Invites.Activate;
using RideTracker.Rides.DaysSummary;
using RideTracker.Rides.Details;
using RideTracker.Rides.History;
using RideTracker.Rides.ListOfDays;
using RideTracker.Rides.Synchronization;
using RideTracker.Vehicles;
using RideTracker.Vehicles.Synchronization;
using RideTracker.Vehicles.VehicleDetails;
using RideTracker.Vehicles.VehicleList;
using Syncfusion.Maui.Toolkit.Hosting;
using ITimer = RideTracker.Utilities.ITimer;
using Timer = RideTracker.Utilities.Timer;

namespace RideTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

            var httpHandler = new HttpClientHandler();
#if DEBUG
            builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
            httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
#endif
            builder.Services.AddSingleton<GroupDetailsViewModel>();
            builder.Services.AddSingleton<AppConfiguration>();
            builder.Services.AddSingleton<GroupUtils>();
            builder.Services.AddSingleton<AlertsService>();
            builder.Services.AddSingleton<RideTrackerHttpClient>();
            builder.Services.AddSingleton<JobRegistry>();
            builder.Services.AddSingleton<SynchronizationJob>();
            builder.Services.AddSingleton<JobRegistry>();
            builder.Services.AddSingleton<GroupsSynchronizer>();
            builder.Services.AddSingleton<VehiclesSynchronizer>();
            builder.Services.AddSingleton<RidesSynchronizer>();
            builder.Services.AddSingleton(typeof(DbLogger<>));
            builder.Services.AddSingleton(new HttpClient(httpHandler));
            builder.Services.AddPersistence();
            builder.Services.AddHttpClientWithRetry();

            builder.Services.AddTransient<VehicleListPage>();
            builder.Services.AddTransient<GroupsListPage>();
            builder.Services.AddTransient<GroupListModel>();
            builder.Services.AddTransient<VehicleListModel>();

            builder.Services.AddTransientWithShellRoute<SignInPage, SignInModel>(nameof(SignInPage));
            builder.Services.AddTransientWithShellRoute<SignUpPage, SignUpModel>(nameof(SignUpPage));
            //builder.Services.AddTransientWithShellRoute<GroupsListPage, GroupListModel>(nameof(GroupsListPage));
            builder.Services.AddTransientWithShellRoute<GroupDetailsPage, GroupDetailsViewModel>(nameof(GroupDetailsPage));
            //builder.Services.AddSingletonWithShellRoute<VehicleListPage, VehicleListModel>(nameof(VehicleListPage));
            builder.Services.AddTransientWithShellRoute<VehicleDetailsPage, VehicleDetailsViewModel>(nameof(VehicleDetailsPage));
            builder.Services.AddTransientWithShellRoute<RideDetailsPage, RideDetailsViewModel>(nameof(RideDetailsPage));
            builder.Services.AddTransientWithShellRoute<DaysSummaryPage, DaysSummaryViewModel>(nameof(DaysSummaryPage));
            builder.Services.AddTransientWithShellRoute<HistoryForOneDayPage, RideHistoryViewModel>(nameof(HistoryForOneDayPage));
            builder.Services.AddTransientWithShellRoute<CreateInvitePage, CreateInviteViewModel>(nameof(CreateInvitePage));
            builder.Services.AddTransientWithShellRoute<ActivateInvitePage, ActivateInviteViewModel>(nameof(ActivateInvitePage));

            builder.Services.AddFirebaseAuthentication();
            builder.Services.AddSingleton(TimeProvider.System);
            builder.Services.AddSingleton<ITimer, Timer>();
            builder.Services.AddSingleton<RideHistoryHelper>();
            builder.Services.AddSingleton<VehicleHelper>();

            return builder.Build();
        }
    }
}
