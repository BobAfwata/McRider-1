using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using McRider.Common.Services;
using McRider.MAUI.Services;
using Binder = Android.OS.Binder;

namespace McRider.MAUI.Platforms.Android.Services;


[Service]
public class AndroidBackgroundService : Service, IBackgroundService
{
    private const int FOREGROUND_ID = 19643;

    private string message, title;
    private ServiceWorker _worker;
    private Notification.Builder notificationBuilder;

    private ILogger _logger;
    IBinder _binder;

    public AndroidBackgroundService() : this(
        App.ServiceProvider.GetService<ServiceWorker>(),
        App.ServiceProvider.GetService<ILogger<ServiceWorker>>()
    )
    { }

    public AndroidBackgroundService(ServiceWorker worker, ILogger<ServiceWorker> logger)
    {
        _worker = worker;
        _logger = logger;
        _binder = new LocalBinder(this);
    }

    public override IBinder? OnBind(Intent? intent)
    {
        return _binder;
    }

    //we catch the actions intents to know the state of the foreground service
    [return: GeneratedEnum]
    public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
    {
        if (intent.Action == "START_SERVICE")
        {
            try
            {
                var isNotified = false;
                var progress = new Progress<double>(p =>
                {
                    _logger.LogInformation(message + $" {p}%");

                    if (!isNotified || notificationBuilder == null)
                    {
                        isNotified = true;
                        RegisterNotification(); //Proceed to notify
                    }

                    if (p >= 99.999)
                    {
                        notificationBuilder?.SetProgress(0, 0, false);
                    }
                    else
                    {
                        notificationBuilder?.SetProgress(100, (int)p, false);
                    }
                });

                Task.Run(async () =>
                {
                    _logger.LogInformation($"Running background processes..");
                    var count = await _worker.Start(progress);
                    _logger.LogInformation($"Processed {count} records in the background!");

                    this.Stop();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running Sync!");
                this.Stop();
            }
        }
        else if (intent.Action == "STOP_SERVICE")
        {
            _worker.Stop();
            StopForeground(true);//Stop the service
            StopSelfResult(startId);
        }

        // Return the sticky flag to tell the system to keep the service running in the background
        return StartCommandResult.Sticky;
    }

    public void Start(string title = null, string message = null)
    {
        // if (!MainApplication.IsInForeground) return;

        title ??= Guid.NewGuid().ToString();
        message ??= "Processing..";

        this.message = message;
        this.title = title;

        Intent startService = new Intent(MainApplication.MainActivity, typeof(AndroidBackgroundService)).SetAction("START_SERVICE");
        MainApplication.MainActivity.StartService(startService);
    }

    public void Stop()
    {
        Intent stopIntent = new Intent(MainApplication.MainActivity, typeof(AndroidBackgroundService)).SetAction("STOP_SERVICE");
        MainApplication.MainActivity.StartService(stopIntent);
    }

    private void RegisterNotification()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel("ServiceChannel", nameof(AndroidBackgroundService), NotificationImportance.Max);
            var manager = (NotificationManager)MainApplication.MainActivity.GetSystemService(Context.NotificationService);

            manager.CreateNotificationChannel(channel);
        }

        notificationBuilder = new Notification.Builder(this, "ServiceChannel")
           .SetContentTitle(title ?? "Syncing data..")
           .SetContentText(message ?? "Please wait, this might take some time...")
           .SetProgress(100, 0, true)
           //.SetSmallIcon(Resource.Drawable.splash)
           .SetOngoing(true);

        var notification = notificationBuilder.Build();

        StartForeground(FOREGROUND_ID, notification);
    }
}

public class LocalBinder : Binder
{
    AndroidBackgroundService _service;

    public LocalBinder(AndroidBackgroundService service)
    {
        _service = service;
    }

    public Service Service => _service;
}
