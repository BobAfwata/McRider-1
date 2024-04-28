using Android.App;
using Android.Content;
using Android.Runtime;

namespace McRider.MAUI
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public static Context ApplicationContext { get; internal set; }

        public static Activity MainActivity { get; internal set; }

        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        { }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
