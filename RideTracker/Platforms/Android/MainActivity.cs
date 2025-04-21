using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using static RideTracker.MainActivity;

namespace RideTracker;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{      
}

public class BatteryOptimizationService : IBatteryOptimizationService
{
    public void RequestIgnoreBatteryOptimizations()
    {
        var context = Android.App.Application.Context;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            string packageName = context.PackageName;

            if (!pm.IsIgnoringBatteryOptimizations(packageName))
            {
                Intent intent = new Intent(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
                intent.SetData(Android.Net.Uri.Parse("package:" + packageName));
                intent.SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
        }
    }
}


public interface IBatteryOptimizationService
{
    void RequestIgnoreBatteryOptimizations();
}
