using Hast.Communication.Exceptions;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
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
        /// <returns>The serial port name where the FPGA board is connected to.</returns>
        public static async Task<Task<string>> GetFpgaPortName()
        {
            // Get all available serial ports on system.
            var ports = SerialPort.GetPortNames();
            // If no serial ports detected, then throw an Exception.
            if (ports == null) throw new SerialPortCommunicationException("No serial port detected.");

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
                    serialPort.Dispose();
                    taskCompletionSource.SetResult(serialPort.PortName);
                }
            };

            for (int i = 0; i < ports.Length; i++)
            {
                serialPort.PortName = ports[i];

                try
                {
                    serialPort.Open();
                    serialPort.Write(Constants.FpgaConstants.SignalFpgaDetect);
                }
                catch (IOException e) { } 
            }

            if (!taskCompletionSource.Task.IsCompleted) // Do not wait unnecessarily if the FPGA board is already detected.
            {
                await Task.Delay(5000); // Wait 5 seconds.
                if (!taskCompletionSource.Task.IsCompleted) // If the last serial port didn't responded, then throw an Exception.
                {
                    throw new SerialPortCommunicationException("FPGA board not detected.");
                }
            }
            
            await taskCompletionSource.Task;
            return taskCompletionSource.Task;
        }
    }
}
