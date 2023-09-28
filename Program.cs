using System;
using System.IO.Ports;

namespace COMPortTerminal
{
    class Program
    {
        static void Main(string[] args)
        {
            string comPortName = "COM4"; // Change to your desired COM port
            int baudRate = 921600;       // Change to your desired baud rate

            try
            {
                SerialPort serialPort = new SerialPort(comPortName, baudRate);

                serialPort.DataReceived += SerialPort_DataReceived;

                serialPort.Open();

                Console.WriteLine($"Connected to {comPortName} at {baudRate} Bd. Press Enter to exit.");

                Console.ReadLine();

                serialPort.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            string data = serialPort.ReadExisting();
            Console.Write(data);
        }
    }
}
