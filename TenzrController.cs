using System.IO.Ports;

namespace COMPortTerminal {
    class TenzrController {
        private SerialPort _serialPort;
        private string _comPortName;
        private int _baudRate;
        private bool _processRunning;
        private string _directoryPath;
        private string _formattedDateTime;
        
        private TenzrController() {
            _comPortName = "COM9"; // Manually adjust the COM PORT
            _baudRate = 921600; // Manually adjust the baud rate / speed
            _serialPort = new SerialPort(_comPortName, _baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                DtrEnable = true
            };
            _serialPort.DataReceived += serialPort_DataReceived;
            _processRunning = false;
            _directoryPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? Directory.GetCurrentDirectory(), "exportedData");
            _formattedDateTime = "";

        }

        private void OpenSerialPort() {
            try {
                _serialPort.Open();
                _processRunning = true;
                Console.WriteLine($"Connected to {_comPortName} at {_baudRate} Bd. Press Enter to exit.");
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
                CloseSerialPort();
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
            try {
                while (_serialPort.BytesToRead > 0) {
                    string receivedData = _serialPort.ReadLine();

                    // save to CSV when in streaming mode
                    if (_formattedDateTime != "") {
                        saveToCsv(receivedData);
                    }
                    //else just write to Console
                    else {
                        Console.WriteLine(receivedData);
                    }
                }
                
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
                CloseSerialPort();
            } 
            
        }

        private void saveToCsv(string receivedData) {
            try {
                string filePath = Path.Combine(_directoryPath, $"{_formattedDateTime}.csv");

                // Create the directory if it does not exist
                if (!Directory.Exists(_directoryPath)) {
                    Directory.CreateDirectory(_directoryPath);
                }

                // Ensure the file exists or create a new one
                using StreamWriter sw = File.AppendText(filePath);
                // Write the received data to the CSV file
                sw.Write(receivedData);

            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
                CloseSerialPort();
            } 
        }
 
        private void SendCommand(string command) {
            try {

                // save to CSV only when streaming.
                if (command == "$stream;") {
                    DateTime currentDateTime = DateTime.Now;
                    _formattedDateTime = currentDateTime.ToString("yyyy-MM-dd_HH-mm-ss");
                }
                else {
                    _formattedDateTime = "";
                }

                if (_serialPort.IsOpen) {
                    _serialPort.Write(command);
                }
                else {
                    Console.WriteLine("Serial port is not open. Cannot send the command.");
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
                CloseSerialPort();
            }
        }

        static bool validCommand(string command) {
            // Check if the command is not null and has at least two characters
            if (string.IsNullOrWhiteSpace(command) || command.Length < 2) {
                return false;
            }
            // Check if the first character is '$' and the last character is ';'
            if (command[0] == '$' && command[^1] == ';') {
                if (command.Contains("menu")) {
                    return command.Length == 6;
                }
                else if (command.Contains("stream")) {
                    return command.Length == 8;
                }
                else if (command.Contains("exit")) {
                    return command.Length == 6;
                }
            }
            return false;
        }

        static void Main(string[] args) {
            TenzrController tenzrController = new TenzrController();

            tenzrController.OpenSerialPort();
            
            while (tenzrController._processRunning) {
                Console.WriteLine("Enter a command ($<command>;) or $exit; to exit:");
                string? command = Console.ReadLine();
                
                bool commandValid = command != null && validCommand(command);

                if (commandValid && command != null) {
                    if (command == "$exit;") {
                        Console.WriteLine("Exiting the TenzrController.");
                        tenzrController._processRunning = false;
                        break;
                    } else {
                        tenzrController.SendCommand(command);
                    }
                }
                else {
                    Console.WriteLine("Command Invalid");
                }
            }
            tenzrController.CloseSerialPort();
        }
    }
}
