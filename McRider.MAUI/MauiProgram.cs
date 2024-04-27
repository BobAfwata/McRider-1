using CommunityToolkit.Maui;
using McRider.Common.Extensions;
using McRider.Common.Logging;
using McRider.MAUI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

[assembly: AssemblyVersion("1.0.*")]
namespace McRider.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
                fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
            });

        builder.Logging.AddProvider(new CustomLoggerProvider(NLogConfigFile, GetLogData));
        builder.Services.AddServices();   //Add Other services
#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            // Make sure to add "using Microsoft.Maui.LifecycleEvents;" in the top of the file 
            events.AddWindows(windowsLifecycleBuilder =>
            {
                windowsLifecycleBuilder.OnWindowCreated(window =>
                {
                    window.ExtendsContentIntoTitleBar = false;
                    var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
                    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
                    App.ServiceProvider.GetService<IScreenSelector>()?.MoveCurrentToProtraitScreen(handle);

                    switch (appWindow.Presenter)
                    {
                        case Microsoft.UI.Windowing.OverlappedPresenter overlappedPresenter:
                            overlappedPresenter.SetBorderAndTitleBar(false, false);
                            overlappedPresenter.Maximize();
                            break;
                    }
                });
            });
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }


    public static string NLogConfigFile => $"{App.AppName}.Resources.Configs.nlog.{DeviceInfo.Platform.ToString().ToLower()}.config";

    public static IDictionary<string, object> GetLogData()
    {
        var logdata = new Dictionary<string, object>()
        {
            { "Device", DeviceInfoDict },
            { "App", AppInfoDict }
        };

        return logdata;
    }

    #region Log Info IDictionary
    public static IDictionary<string, string> AppInfoDict
    {
        get
        {
            var obj = new
            {
                Name = App.AppName,
                BuildVersion = App.AppVersion,
                BuildDate = App.AppBuildDate
            };

            return obj.ToDictionary().ToDictionary(d => d.Key, d => d.Value != null ? Newtonsoft.Json.JsonConvert.SerializeObject(d.Value) : null);
        }
    }

    public static IDictionary<string, string> DeviceInfoDict
    {
        get
        {
            var obj = new
            {
                Name = DeviceInfo.Name,
                Model = DeviceInfo.Model,
                Os = DeviceInfo.Platform.ToString(),
                Version = DeviceInfo.VersionString,
                Manufacturer = DeviceInfo.Manufacturer,
                IPAddress = App.IPAddress
            };

            return obj.ToDictionary().ToDictionary(d => d.Key, d => d.Value != null ? Newtonsoft.Json.JsonConvert.SerializeObject(d.Value) : null);
        }
    }
    #endregion
}
