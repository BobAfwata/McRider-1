using McRider.Common.Services;
using McRider.MAUI.Services;
using System.IO.Ports;

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
        _serialPort = new SerialPort();

        try
        { 
            _serialPort.Open();
            return true;
        }
        catch (System.IO.IOException ex)
        {
            _logger.LogError(ex, "Error opening serial port!");
#if DEBUG
            return true;
#endif
            return false;
        }
    }

    public override Task Start(Matchup matchup)
    {
        _serialPort.PortName = _configs?.PortName ?? "COM3";
        _serialPort.BaudRate = _configs?.BaudRate ?? 9600;
        _serialPort.ReadTimeout = _configs?.ReadTimeout ?? 500;

        

        return base.Start(matchup);
    }

    public override Task Stop()
    {
        _serialPort.Close();
        return base.Stop();
    }

    public override string ReadData()
    {
        return _serialPort.IsOpen ? _serialPort.ReadLine() : string.Empty;
    }

    public override void SendData(string data)
    {
        if (_serialPort.IsOpen)
        {
            _serialPort.WriteLine(data);
        }
    }
}
