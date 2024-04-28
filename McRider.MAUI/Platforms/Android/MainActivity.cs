using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;

namespace McRider.MAUI
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnStart()
        {
            //Set some static properties
            MainApplication.MainActivity = this;
            MainApplication.ApplicationContext = ApplicationContext;

            base.OnStart();

            // Global Error handling
            TaskScheduler.UnobservedTaskException += (a, e) => App.OnGlobalException(a, e?.Exception);
            AndroidEnvironment.UnhandledExceptionRaiser += (a, e) => App.OnGlobalException(a, e?.Exception);
        }
    }
}
