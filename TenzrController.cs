using System.IO.Ports;
using System.Text.RegularExpressions;

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

                    // save to CSV when in streaming mode and ignore all empty lines
                    if (_formattedDateTime != "" && receivedData != "\n") {
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
                if (_serialPort.IsOpen) {
                    _serialPort.Write(command);
                    // save to CSV only when streaming.
                    if (command == "$stream;") {
                        DateTime currentDateTime = DateTime.Now;
                        _formattedDateTime = currentDateTime.ToString("yyyy-MM-dd_HH-mm-ss");
                    }
                    else {
                        Thread.Sleep(500);
                        _formattedDateTime = "";
                    }
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

        static bool validCommand(string command, TenzrController tenzrController) {
            try {
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
                    else if (command.Contains("freq")) {
                        string pattern = @"^\$freq, (\d+);";
                        // Check if the command matches the pattern and frequency is within the specified range
                        Match match = Regex.Match(command, pattern);
                        if (match.Success) {
                            int frequency;
                            if (int.TryParse(match.Groups[1].Value, out frequency)) {
                                return frequency >= 1 && frequency <= 100;
                            }
                        }
                    }
                    else if (command.Contains("axis")) {
                        string pattern = @"^\$axis, (pitch|roll|yaw);";
                        // Check if the command matches the pattern and axis is within the specified options
                        Match match = Regex.Match(command, pattern);
                        return match.Success;
                    }
                    else if (command.Contains("ref") || command.Contains("sig")) {
                        // Define the regex pattern
                        string pattern = @"^\$(ref|sig), (-?\d+), (-?\d+), (-?\d+);";
                        Match match = Regex.Match(command, pattern);
                        if (match.Success) {
                            int roll, pitch, yaw;
                            // Attempt to parse the signed integers
                            if (int.TryParse(match.Groups[2].Value, out roll) &&
                                int.TryParse(match.Groups[3].Value, out pitch) &&
                                int.TryParse(match.Groups[4].Value, out yaw)) {
                                // The integers are valid
                                return true;
                            }
                        }
                    }
                    else {
                        return false;
                    }
                }
                return false;
            } catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
                tenzrController.CloseSerialPort();
                return false;
            }
        }

        static void Main(string[] args) {
            TenzrController tenzrController = new TenzrController();

            tenzrController.OpenSerialPort();
            
            while (tenzrController._processRunning) {
                Console.WriteLine("Enter a command ($<command>;) or $exit; to exit:");
                string? command = Console.ReadLine();
                
                bool commandValid = command != null && validCommand(command, tenzrController);

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
                    if (tenzrController._serialPort.IsOpen) {
                        Console.WriteLine("Command Invalid");
                    }
                }
            }
            tenzrController.CloseSerialPort();
        }
    }
}
