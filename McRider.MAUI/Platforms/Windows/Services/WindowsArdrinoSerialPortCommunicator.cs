using McRider.Common.Services;
using McRider.MAUI.Services;
using System.IO.Ports;
using System.Threading;

namespace McRider.MAUI.Platforms.Windows.Services;

public class WindowsArdrinoSerialPortCommunicator : ArdrinoCommunicator
{
    private SerialPort _serialPort;

    public WindowsArdrinoSerialPortCommunicator(FileCacheService cacheService, ILogger<WindowsArdrinoSerialPortCommunicator> logger) : base(cacheService)
    {
        _logger = logger;
    }

    public override async Task<bool> Initialize()
    {
        await base.Initialize();

        if (_serialPort != null)
        {
            if (_detectPortTask != null)
                await _detectPortTask;

            return true;
        }

        _serialPort = new SerialPort();

        // Detect port if not set or modified more than 24 hours ago
        if ((DateTime.UtcNow - _configs.ModifiedTime).TotalHours > 24)
            await DetectPort();

        _serialPort.PortName = _configs?.PortName ?? "COM4";
        _serialPort.BaudRate = _configs?.BaudRate ?? 9600;
        _serialPort.ReadTimeout = _configs?.ReadTimeout ?? 500;

        int count = 0;
        do
        {
            try
            {
                _serialPort.Open();
                return true;
            }
            catch (System.IO.IOException ex)
            {
                _logger.LogError(ex, "Error opening serial port!");
                await DetectPort();
            }
        } while (count++ < 1);

#if DEBUG
        return true;
#endif
        return false;
    }


    private Task? _detectPortTask = null;
    private Task DetectPort()
    {
        if (_detectPortTask != null)
            return _detectPortTask;

        _detectPortTask = Task.Run(async () =>
        {
            var ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                try
                {
                    _serialPort = new SerialPort(port, _configs.BaudRate);
                    _serialPort.ReadTimeout = _configs.ReadTimeout;
                    _serialPort.Open();

                    var message = await ReadDataAsync();
                    if (string.IsNullOrEmpty(message))
                    {
                        _serialPort?.Close();
                        continue;
                    }

                    var json = JObject.Parse(message);
                    if (json["distance_1"] != null || json["bikeA"] != null)
                    {
                        _configs.PortName = port;
                        _configs.ModifiedTime = DateTime.UtcNow;
                        await _cacheService.SetAsync("configs.json", _configs);
                        break;
                    }
                    else
                    {
                        _serialPort?.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error accessing port {port}");
                }
            }
        }).ContinueWith(task => _detectPortTask = null);

        return _detectPortTask;
    }

    public override Task Start(Matchup matchup)
    {
        return base.Start(matchup);
    }

    public override Task Stop()
    {
        _serialPort.Close();
        return base.Stop();
    }

    public async override Task DoReadDataAsync()
    {
        await Initialize();
#if DEBUG1
        if (_serialPort.IsOpen != true)
            await DoFakeReadData();
        else
#endif
            await base.DoReadDataAsync();
    }

    public override async Task<string?> ReadDataAsync(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(1.2);
        _serialPort.ReadTimeout = (int)(timeout?.TotalMilliseconds ?? 1000);

        var cancellationTokenSource = new CancellationTokenSource();
        var readTask = Task.Run(() =>
        {
            try
            {
                return _serialPort?.ReadLine() ?? "";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading " + _serialPort.PortName + "!!");
                return null;
            }
        }, cancellationTokenSource.Token);

        var completedTask = await Task.WhenAny(readTask, Task.Delay(timeout.Value, cancellationTokenSource.Token));

        if (completedTask == readTask)
            return await readTask;

        cancellationTokenSource.Cancel(); // Cancel the read task
        return null;
    }

    public override void SendData(string data)
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.WriteLine(data);
        }
    }
}
