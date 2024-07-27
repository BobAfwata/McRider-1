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

        if (_detectPortTask != null)
            await _detectPortTask;

        if (_serialPort?.IsOpen == true)
            return true;

        // Detect port if not set or modified more than 24 hours ago
        if ((DateTime.UtcNow - _configs.ModifiedTime).TotalHours > 24)
            await DetectPort();

        if (_serialPort?.IsOpen != true)
        {
            _serialPort ??= new SerialPort();
            _serialPort.PortName = _configs?.PortName ?? "COM4";
            _serialPort.BaudRate = _configs?.BaudRate ?? 9600;
            _serialPort.ReadTimeout = _configs?.ReadTimeout ?? 500;
        }

        if (_serialPort.IsOpen != true)
        {
            int count = 0;
            do
            {
                try
                {
                    _serialPort.Open();
                    break;
                }
                catch (System.IO.IOException ex)
                {
                    _logger.LogError(ex, "Error opening serial port!");
                    await DetectPort();
                }
            } while (count++ < 1);
        }

        // for DEBUG
        if (_configs?.FakeRead == true && _serialPort?.IsOpen != true)
        {
            _serialPort = null;
            _detectPortTask = Task.CompletedTask;
            return true;
        }

        return _serialPort?.IsOpen == true;
    }


    private Task? _detectPortTask = null;
    private Task DetectPort()
    {
        if (_detectPortTask != null)
            return _detectPortTask;

        _detectPortTask = Task.Run(async () =>
        {
            var ports = SerialPort.GetPortNames();
            _serialPort = null;

            foreach (string port in ports)
            {
                try
                {
                    _serialPort = new SerialPort(port, _configs.BaudRate);
                    _serialPort.ReadTimeout = _configs.ReadTimeout + 10;
                    _serialPort.Open();

                    if(await ValidateSerialPort() != true)
                    {
                        _serialPort?.Close();
                        _serialPort = null;
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error accessing port {port}");
                    if (_serialPort?.IsOpen == true)
                        _serialPort.Close();
                    _serialPort = null;
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
        if (_serialPort?.IsOpen == true)
            _serialPort.Close();
        return base.Stop();
    }

    public async override Task DoReadDataAsync()
    {
        await Initialize();

        if (await ValidateSerialPort())
            await base.DoReadDataAsync();
        else if (_configs?.FakeRead == true)
            await DoFakeReadData();
        else
            _logger.LogError("Serial port is not open!");
    }

    private async Task<bool> ValidateSerialPort()
    {
        if (_serialPort?.IsOpen != true)
            return false;

        var timeout = TimeSpan.FromMilliseconds(_configs.ReadTimeout + 10);
        var message = await ReadDataAsync(timeout, -1);
        if (string.IsNullOrEmpty(message))
            return false;

        var json = JObject.Parse(message);
        if (json["distance_1"] != null || json["bikeA"] != null)
            return true;

        return false;
    }

    public override async Task<string?> ReadDataAsync(TimeSpan? timeout = null, int retryCount = 0)
    {
        timeout ??= TimeSpan.FromSeconds(1.2);
        _serialPort.ReadTimeout = (int)(timeout?.TotalMilliseconds ?? 1000);

        var cancellationTokenSource = new CancellationTokenSource();
        var readTask = Task.Run(async () =>
        {
            try
            {
                if (_serialPort?.IsOpen != true)
                    _serialPort?.Open();

                return _serialPort?.ReadLine() ?? "";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while reading from _serialPort " + (_serialPort?.PortName) + "!!");
                if (retryCount < 0 || retryCount >= 10)
                    return null;

                await Task.Delay(1000);
                return await ReadDataAsync(timeout + TimeSpan.FromMilliseconds(100), retryCount + 1);
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
