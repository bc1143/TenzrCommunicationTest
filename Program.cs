using System.IO.Ports;


namespace COMPortTerminal {
    class Program {
        static SerialPort serialPort;

        static void Main(string[] args) {
            string comPortName = "COM9"; // Change to your desired COM port
            int baudRate = 921600;       // Change to your desired baud rate

            try {
                serialPort = new SerialPort(comPortName, baudRate);

                serialPort.DataReceived += SerialPort_DataReceived;

                serialPort.Open();

                Console.WriteLine($"Connected to {comPortName} at {baudRate} Bd. Press Enter to exit.");

                while (true) {
                    Console.WriteLine("Enter a command to send (or press Enter to exit):");
                    string command = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(command)) 
                        break;
                    
                    Console.WriteLine(command);

                    try {
                        SendCommand(command);
                    }   
                    catch (Exception ex) {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                if (serialPort.IsOpen && serialPort != null) {
                    serialPort.Close();
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e) {
            SerialPort receivedPort = (SerialPort)sender;
            string data = receivedPort.ReadExisting();
            Console.Write(data);
        }

        private static void SendCommand(string command) {
            if (serialPort.IsOpen) {
                serialPort.Write(command);
                Console.WriteLine($"Sent: {command}");
            }
            else {
                Console.WriteLine("Serial port is not open. Cannot send the command.");
            }
        }
    }
}
