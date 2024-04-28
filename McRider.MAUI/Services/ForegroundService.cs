using McRider.Common.Services;
using McRider.MAUI.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Services;


public class ForegroundService : IForegroundService, IBackgroundService
{
    private ServiceWorker _worker;
    private ILogger _logger;

    public ForegroundService(ServiceWorker worker, ILogger<ForegroundService> logger)
    {
        _worker = worker;
        _logger = logger;
    }

    public void Start(string title = null, string message = null)
    {
        if (_worker.IsRunning) return;
        title ??= Guid.NewGuid().ToString();
        message ??= "Processing..";

        _ = Task.Run(async () =>
        {
            var progress = new Progress<double>(p =>
            {
                _logger.LogInformation($"{title} {message} {p}%");
                WeakReferenceMessenger.Default.Send(new ProgressMessage { Title = title, Message = title, Progress = p });
            });

            var count = await _worker.Start(progress);
            _logger.LogInformation($"Processed {count} records in the background!");
        });
    }

    public void Stop()
    {
        _worker.Stop();
    }
}
