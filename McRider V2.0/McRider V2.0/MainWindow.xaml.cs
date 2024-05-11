using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace McRider_V2._0
{
    public partial class MainWindow : Window
    {
        SerialPort port = new SerialPort();
        private readonly object lockObject = new object();

        public MainWindow()
        {
            InitializeComponent();

            try
            {
                port.BaudRate = 9600;
                port.PortName = "COM3"; // Replace with your actual COM port
                port.Open();

                // Start a new thread to read and display data
                Thread thread = new Thread(ReadAndDisplayData);
                thread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening serial port: {ex.Message}");
            }
        }

        private void ReadAndDisplayData()
        {
            try
            {
                while (true)
                {
                    string jsonData = port.ReadLine();
                    JObject json = JObject.Parse(jsonData);

                    // Use Dispatcher.Invoke to update UI from a non-UI thread
                    Dispatcher.Invoke(() =>
                    {
                        label1.Content = $"Time 1: {json["time_1"]}s";
                        label2.Content = $"Distance 1: {json["distance_1"]}";
                        label3.Content = $"Time 2: {json["time_2"]}s";
                        label4.Content = $"Distance 2: {json["distance_2"]}";
                    });

                    // Add a small delay to avoid excessive UI updates
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading from serial port: {ex.Message}");
            }
        }
    }
}
