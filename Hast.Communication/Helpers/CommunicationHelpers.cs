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

        /// <summary>
        /// Helper Method used for detection of the connected FPGA board.
        /// </summary>
        /// <returns>The COM port name where the FPGA board is connected.</returns>
        public static Task<string> GetFpgaPortName()
        {
            // Get all available serial ports on system.
            var ports = SerialPort.GetPortNames();
            var serialPort = new SerialPort();

            serialPort.BaudRate = Constants.FpgaConstants.BaudRate;
            serialPort.Parity = Constants.FpgaConstants.SerialPortParity;
            serialPort.StopBits = Constants.FpgaConstants.SerialPortStopBits;
            serialPort.WriteTimeout = Constants.FpgaConstants.WriteTimeoutInMilliseconds;

            var taskCompletionSource = new TaskCompletionSource<string>();
            serialPort.DataReceived += (s, e) => 
            {
                var dataIn = (byte)serialPort.ReadByte();
                var receivedCharacter = Convert.ToChar(dataIn);

                if (receivedCharacter == Constants.FpgaConstants.SignalYes)
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
                    serialPort.Write(Constants.FpgaConstants.SignalFpgaDetect);
                }
                catch (IOException e) { }
            }

            return taskCompletionSource.Task;
        }
    }
}
