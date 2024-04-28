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
     
    public override async Task Initialize()
    {
        await base.Initialize();

        _serialPort = new SerialPort();
        _serialPort.PortName = _configs?.PortName ?? "COM3";
        _serialPort.BaudRate = _configs?.BaudRate ?? 9600;
        _serialPort.ReadTimeout = _configs?.ReadTimeout ?? 500;
        _serialPort.Open();
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
