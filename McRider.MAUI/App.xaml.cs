using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using McRider.Common.Services;
using McRider.MAUI.Extensions;
using McRider.MAUI.Services;
using System.Runtime.CompilerServices;

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
            ServiceProvider.GetService<IBackgroundService>()?.Start();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Stop ForegroundService, Start BackgroundService
            //ServiceProvider.GetService<IForegroundService>()?.Stop();
            //ServiceProvider.GetService<IBackgroundService>()?.Start();
        }

        protected override void OnSleep()
        {
            base.OnSleep();

            // Stop BackgroundService, Start ForegroundService
            //ServiceProvider.GetService<IBackgroundService>()?.Stop();
            //ServiceProvider.GetService<IForegroundService>()?.Start();
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

            // Delay for a short period
            await Task.Delay(1000);

            // Redirect to SliderPage
            await Shell.Current.GoToAsync($"///{nameof(SliderPage)}");
        }
        #endregion

        /// <summary>
        /// Runs a task in the background/foreground service
        /// Can be used interchangably with Device.StartTimer but more flexible
        /// </summary>
        /// <param name="timeSpan">Interval between which to retry the task</param>
        /// <param name="action">
        ///     - Return true to keep repeating after the set interval
        ///     - Return false task was completed no farther action needed
        /// </param>
        public static void StartBackgroundTimer(TimeSpan timeSpan, Func<Boolean> action)
        {
            var worker = ServiceProvider.GetService<ServiceWorker>();

            worker?.AddAction(async p =>
            {
                await Task.Delay(timeSpan);
                return action?.Invoke() == true; // True = Repeat again, False = Stop the timer
            });
        }

        /// <summary>
        /// Runs a task in the background/foreground service
        /// Can be used interchangably with Device.StartTimer but more flexible
        /// </summary>
        /// <param name="timeSpan">Interval between which to retry the task</param>
        /// <param name="action">
        ///     - Return true to keep repeating after the set interval
        ///     - Return false task was completed no farther action needed
        /// </param>
        public static void StartTimer(TimeSpan timeSpan, Func<Boolean> action)
        {
            Timer timer = null;

            // Create a new timer with a interval of 100 milliseconds
            TimerCallback timerCallback = (state) =>
            {
                // Call your method here
                if (action?.Invoke() == true)
                    timer?.Change(timeSpan.Milliseconds, Timeout.Infinite);
                else
                    timer?.Change(Timeout.Infinite, Timeout.Infinite);                    
            };

            timer = new Timer(timerCallback, null, TimeSpan.Zero, timeSpan);
        }

        #region Toasts and Dialog

        public async static Task ShowToast(string message, ToastDuration duration = ToastDuration.Short, double fontSize = 14)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            if (!string.IsNullOrEmpty(message) && App.Current.Resources.TryGetValue(message, out var strValue) && !string.IsNullOrEmpty(strValue?.ToString()))
                message = strValue?.ToString();

            var toast = Toast.Make(message, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token);
        }

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public async static Task<bool> ShowMessage(
            string message,
            string title = "Message",
            string okButtonText = "Ok",
            string cancelButtonText = null//"Cancel"
        )
        {
            if (!string.IsNullOrEmpty(message) && App.Current.Resources.TryGetValue(message, out var strValue) && !string.IsNullOrEmpty(strValue?.ToString()))
                message = strValue?.ToString();

            var view = new MessageDialogView(vm => { }, vm => { }, title, message, okButtonText, cancelButtonText);
            await semaphore.WaitAsync();
            try
            {
                var ok = (await view.ShowPopupAsync()) as Boolean?;
                return ok == true;
            }
            finally
            {
                semaphore.Release();
            }
        }
        #endregion

        #region Global Error handling
        public static string ErrorFilePath
        {
            get
            {
                var currentAssemblyName = string.Join(".", (Assembly.GetExecutingAssembly()?.GetName()?.Name ?? "files.").Split('.').SkipLast(1));

                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    currentAssemblyName, "Exception.log"
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
            _ = App.ShowMessage(e.GetDetails(), "Crash Report!", "Close");
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
