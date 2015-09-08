using System;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Hast.Communication.Helpers
{
    public static class CommunicationHelpers
    {
        /// <summary>
        /// Simple helper method that converts int to byte array.
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public static byte[] ConvertIntToByteArray(int from)
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(from);
                return stream.ToArray();
            }
        }

        public static Task<string> GetFPGAPortName()
        {
            // Get all available serial ports on system.
            var ports = SerialPort.GetPortNames();
            SerialPort serialPort = new SerialPort();

            serialPort.BaudRate = Constants.FpgaConstants.BaudRate;
            serialPort.Parity = Constants.FpgaConstants.SerialPortParity;
            serialPort.StopBits = Constants.FpgaConstants.SerialPortStopBits;
            serialPort.WriteTimeout = Constants.FpgaConstants.WriteTimeout;

            var taskCompletionSource = new TaskCompletionSource<string>();
            serialPort.DataReceived += (s, e) => {
                var dataIn = (byte)serialPort.ReadByte();
                
                var RXString = Convert.ToChar(dataIn);
                if (RXString == 'y')
                {
                    serialPort.Close();
                    taskCompletionSource.SetResult(serialPort.PortName);
                }
            };

            foreach (var port in ports)
            {
                serialPort.PortName = port;
                
                try
                {
                    serialPort.Open();
                    serialPort.Write("p");
                }
                catch (IOException e) { }
            }

            return taskCompletionSource.Task;
        }

        public static string DetectSerialConnectionsPortName()
        {
            // Get all available serial ports on system.
            var ports = SerialPort.GetPortNames();
            SerialPort serialPort = new SerialPort();

            //Initialize the communication
            serialPort.BaudRate = Constants.FpgaConstants.BaudRate;
            serialPort.Parity = Constants.FpgaConstants.SerialPortParity;
            serialPort.StopBits = Constants.FpgaConstants.SerialPortStopBits;
            serialPort.WriteTimeout = Constants.FpgaConstants.WriteTimeout;

            // Checking the port which the FPGA board is connected.  
            foreach (var port in ports)
            {
                serialPort.PortName = port;

                try
                {
                    serialPort.Open();
                    serialPort.Close();
                    return port;
                }
                catch (IOException e) { }
                finally
                {
                    serialPort.Close();
                }
            }

            return null;
        }
    }
}
