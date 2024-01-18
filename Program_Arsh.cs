using System.IO.Ports;
using System.Text;

namespace COMPortTerminal {
    class TenzrController {
        
        private SerialPort serialPort;
        private string comPortName;
        private int baudRate;
        private bool processRunning;
        private StringBuilder buffer = new StringBuilder();
        
        private TenzrController(string comPortName, int baudRate) {
            this.comPortName = comPortName;
            this.baudRate = baudRate;
            serialPort = new SerialPort(comPortName, baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One
            };
            processRunning = false;
        }

        private void OpenSerialPort() {
            try {
                serialPort.Open();
                processRunning = true;
                serialPort.DataReceived += serialPort_DataReceived;
                Console.WriteLine($"Connected to {comPortName} at {baudRate} Bd. Press Enter to exit.");
                Thread.Sleep(1000);
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
            } finally {
                serialPort.Dispose();
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            Console.WriteLine("I was here");
            while (serialPort.BytesToRead > 0) {
                char c = (char)serialPort.ReadChar();
                buffer.Append(c);

                if (c == '\n') {
                    string receivedData = buffer.ToString().Trim();
                    Console.WriteLine(receivedData);
                    buffer.Clear();
                }
            }
            Console.WriteLine("I was here");
        }
 
        private void SendCommand(string command) {
            try {
                if (serialPort.IsOpen) {
                    serialPort.Write(command);
                    Console.WriteLine("Sent");
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
            
            while (tenzrController.processRunning) {
                Console.WriteLine("Enter a command to send ($stream, $menu):");
                string? command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command) || command == null) {
                    Console.WriteLine("Exiting the TenzrController.");
                    tenzrController.processRunning = false;
                } else {
                    tenzrController.SendCommand(command);
                }
            }

            tenzrController.CloseSerialPort();
        }
    }
}
