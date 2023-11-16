using System.IO.Ports;

namespace COMPortTerminal {
    class TenzrController {
        
        private SerialPort serialPort;
        private string comPortName;
        private int baudRate;
        
        private TenzrController(string comPortName, int baudRate) {
            this.comPortName = comPortName;
            this.baudRate = baudRate;

            serialPort = new SerialPort(comPortName, baudRate);
            serialPort.DataReceived += SerialPortDataReceived;
        }

        private void OpenSerialPort() {
            try {
                serialPort.Open();
                Console.WriteLine($"Connected to {comPortName} at {baudRate} Bd. Press Enter to exit.");
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void CloseSerialPort() {
            try {
                if (serialPort.IsOpen) {
                    serialPort.Close();
                    Console.WriteLine($"Connection with {comPortName} at {baudRate} Bd closed.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e) {
            try {
                string data = serialPort.ReadExisting();
                Console.Write(data);
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void SendCommand(string command) {
            try {
                if (serialPort.IsOpen) {
                    serialPort.Write(command);
                    Console.WriteLine($"Sent: {command}");
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
                
            while (true) {
                Console.WriteLine("Enter a command to send (or press Enter to exit):");
                string? command = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(command) || command == null) {
                    Console.WriteLine("Exiting the TenzrController.");
                    break;
                } else {
                    tenzrController.SendCommand(command);
                }
            }

            tenzrController.CloseSerialPort();
        }
    }
}
