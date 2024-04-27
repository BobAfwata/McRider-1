using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using McRider.Common.Extensions;
using McRider.Common.Services;
using McRider.MAUI.Services;
using Microsoft.VisualBasic;

namespace McRider.MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            base.OnStart();

            // More initializations
            _ = Initialize();

            // Start BackgroundService
            //ServiceProvider.GetService<IBackgroundService>()?.Start();
        }

        public static IServiceProvider ServiceProvider => IPlatformApplication.Current.Services;
        public static ILogger? Logger => ServiceProvider.GetService<ILogger<App>>();
        
        #region Initialization
        private async Task Initialize()
        {
            // Set IP Address
            await SetInternetIP();

            // Check last error exception
            OnCheckException();

            // Set App Information
            Logger?.LogInformation($"App Name: {AppName}");
            Logger?.LogInformation($"App Version: {AppVersion}");
            Logger?.LogInformation($"App Build Date: {AppBuildDate}");
            Logger?.LogInformation($"IP Address: {IPAddress}");

            // Global Error handling
            AppDomain.CurrentDomain.UnhandledException += (a, e) => App.OnGlobalException(a, e?.ExceptionObject as Exception);

            // Redirect to SliderPage
            //await Shell.Current.GoToAsync($"///{nameof(SliderPage)}");
        }
        #endregion

        /// <summary>
        /// Runs a task in the background/foreground service
        /// </summary>
        /// <param name="timeSpan">Interval between which to retry the task</param>
        /// <param name="action">
        ///     - Return true if the task was completed successful and need no more retries.
        ///     - Return false to keep repeating after the set interval    
        /// </param>
        public static void StartTimer(TimeSpan timeSpan, Func<Boolean> action)
        {
            var worker = ServiceProvider.GetService<ServiceWorker>();

            worker?.AddAction(async p =>
            {
                await Task.Delay(timeSpan);
                return action?.Invoke() == true;
            });
        }


        #region Global Error handling
        public static string ErrorFilePath
        {
            get
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    AppAssembly.GetName().Name + "Exception.log"
                );

                return path;
            }
        }

        public async static void OnCheckException()
        {
            var path = ErrorFilePath;
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            File.Delete(path);

            if (!json.IsJSON(out Exception e)) return;

            Logger.LogCritical(e, $"Uncaught Global Exception on last run!");

#if DEBUG
            await Task.Delay(2000).ConfigureAwait(false);
            _ = App.Current.MainPage.DisplayAlert("Crash Report!", e.GetDetails(), "Close");
#endif
        }

        public static void OnGlobalException(object sender, Exception e, [CallerMemberName] string callerName = "")
        {
            if (e == null) return;

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            try
            {
                var json = JsonConvert.SerializeObject(e, settings);
                if (!string.IsNullOrEmpty(json))
                    File.WriteAllText(ErrorFilePath, json);
            }
            catch
            {
                Logger.LogCritical(e, $"Uncaught Global Exception on last run!");
            }

#if DEBUG
            // throw e;
#endif
        }

        #endregion

        #region IP Address
        public static async Task<string> SetInternetIP()
        {
            var url = "https://ifconfig.me/ip";
            //var url = "http://checkip.dyndns.org";

            try
            {
                Logger?.LogInformation($"Loading Ip from {url}");

                // Check IP using DynDNS's service
                using HttpClient client = new HttpClient();
                var htmlResponse = await client.GetStringAsync(url);

                // Use regex to extract IP address without the prefix
                var regex = new Regex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b"); // Match IPv4 address
                var match = regex.Match(htmlResponse);

                if (match.Success)
                    return IPAddress = match.Value;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"Error while reading IP from {url}");
            }

            return IPAddress = "127.0.0.1";
        }

        #endregion

        #region App Information
        public static Assembly AppAssembly => typeof(App).GetTypeInfo().Assembly;
        public static string AppName { get; } = AppAssembly.GetName().Name;
        public static string AppVersion { get; } = AppAssembly.GetName().Version.ToString();
        public static DateTime AppBuildDate { get; } = AppAssembly.GetLastBuildDate();
        public static string IPAddress { get; set; }
        #endregion
    }
}
