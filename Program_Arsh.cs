using System;
using System.IO.Ports;
using System.Threading;

namespace COMPortTerminal {
    class TenzrController {
        
        private SerialPort serialPort;
        private string comPortName;
        private int baudRate;
        private readonly Thread readThread;
        private static bool processRunning;
        
        private TenzrController(string comPortName, int baudRate) {
            this.comPortName = comPortName;
            this.baudRate = baudRate;
            serialPort = new SerialPort(comPortName, baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One
            };
            readThread = new Thread(ReadData);
        }

        private void OpenSerialPort() {
            try {
                serialPort.Open();
                processRunning = true;
                readThread.Start();
                Console.WriteLine($"Connected to {comPortName} at {baudRate} Bd. Press Enter to exit.");
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void CloseSerialPort() {
            try {
                if (serialPort.IsOpen) {
                    readThread.Join();
                    serialPort.Close();
                    Console.WriteLine($"Connection with {comPortName} at {baudRate} Bd closed.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                readThread.Join();
                Environment.Exit(1);
            } finally {
                serialPort.Dispose();
            }
        }

        private void ReadData() {
            while (processRunning) {
                try {
                    if (serialPort.BytesToRead > 0) {
                        string data = serialPort.ReadExisting();
                        Console.WriteLine(data);
                    }
                    else {
                        Thread.Sleep(1000);
                    }  
                } catch (Exception ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                    Environment.Exit(1);
                }
            }
        }
 
        private void SendCommand(string command) {
            try {
                if (serialPort.IsOpen) {
                    serialPort.Write(command);
                }
                else {
                    Console.WriteLine("Serial port is not open. Cannot send the command.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void Main(string[] args) {
            string comPortName = "COM9"; // Change to your desired COM port
            int baudRate = 921600;       // Change to your desired baud rate

            TenzrController tenzrController = new TenzrController(comPortName, baudRate);

            tenzrController.OpenSerialPort();

            int count = 0;
                
            while (processRunning && count == 0) {
                Console.WriteLine("Enter a command to send ($stream, $menu):");
                string? command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command) || command == null) {
                    Console.WriteLine("Exiting the TenzrController.");
                    processRunning = false;
                } else {
                    tenzrController.SendCommand(command);
                    count += 1;
                }
            }

            tenzrController.CloseSerialPort();
        }
    }
}
