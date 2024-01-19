using System.IO.Ports;

namespace COMPortTerminal {
    class TenzrController {
        private SerialPort _serialPort;
        private string _comPortName;
        private int _baudRate;
        private bool _processRunning;
        
        private TenzrController() {
            _comPortName = "COM9"; // Manually adjust the COM PORT
            _baudRate = 921600; // Manually adjust the baud rate / speed
            _serialPort = new SerialPort(_comPortName, _baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.XOnXOff
            };
            _serialPort.DataReceived += serialPort_DataReceived;
            _processRunning = false;
        }

        private void OpenSerialPort() {
            try {
                _serialPort.Open();
                _processRunning = true;
                Console.WriteLine($"Connected to {_comPortName} at {_baudRate} Bd. Press Enter to exit.");
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private void CloseSerialPort() {
            try {
                if (_serialPort.IsOpen) {
                    _serialPort.Close();
                    Console.WriteLine($"Connection with {_comPortName} at {_baudRate} Bd closed.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            } finally {
                _serialPort.Dispose();
            }
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            while (_serialPort.BytesToRead > 0) {
                string receivedData = _serialPort.ReadLine();
                Console.WriteLine(receivedData);
            }
        }
 
        private void SendCommand(string command) {
            try {
                if (_serialPort.IsOpen) {
                    command += "\n";
                    _serialPort.Write(command);
                    Console.Write($"Sent: {command}");
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
            TenzrController tenzrController = new TenzrController();

            tenzrController.OpenSerialPort();
            
            while (tenzrController._processRunning) {
                Console.WriteLine("Enter a command to send ($stream, $menu):");
                string? command = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(command) || command == null) {
                    Console.WriteLine("Exiting the TenzrController.");
                    tenzrController._processRunning = false;
                } else {
                    tenzrController.SendCommand(command);
                }
            }

            tenzrController.CloseSerialPort();
        }
    }
}
