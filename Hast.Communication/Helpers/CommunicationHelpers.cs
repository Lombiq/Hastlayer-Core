using Hast.Communication.Exceptions;
using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Communication.Helpers
{
    internal static class CommunicationHelpers
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
        /// Helper method used for detection of the connected FPGA board.
        /// </summary>
        /// <returns>The serial port name where the FPGA board is connected to.</returns>
        public static async Task<string> GetFpgaPortName()
        {
            // Get all available serial ports on system.
            var ports = SerialPort.GetPortNames();
            // If no serial ports detected, then throw an SerialPortCommunicationException.
            if (ports.Length == 0) throw new SerialPortCommunicationException("No serial port detected (no serial ports are open).");

            var serialPort = new SerialPort();
            serialPort.BaudRate = Constants.SerialCommunicationConstants.BaudRate;
            serialPort.Parity = Constants.SerialCommunicationConstants.SerialPortParity;
            serialPort.StopBits = Constants.SerialCommunicationConstants.SerialPortStopBits;
            serialPort.WriteTimeout = Constants.SerialCommunicationConstants.WriteTimeoutInMilliseconds;

            var taskCompletionSource = new TaskCompletionSource<string>();
            serialPort.DataReceived += (s, e) =>
            {
                var dataIn = (byte)serialPort.ReadByte();
                var receivedCharacter = Convert.ToChar(dataIn);

                if (receivedCharacter == Constants.SerialCommunicationConstants.Signals.Yes)
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
                    serialPort.Write(Constants.SerialCommunicationConstants.Signals.FpgaDetect);
                }
                catch (IOException) { }
            }

            if (!taskCompletionSource.Task.IsCompleted) // Do not wait unnecessarily if the FPGA board is already detected.
            {
                await Task.Delay(5000); // Wait 5 seconds.
                if (!taskCompletionSource.Task.IsCompleted) // If the last serial port didn't respond, then throw a SerialPortCommunicationException.
                {
                    serialPort.Dispose();
                    throw new SerialPortCommunicationException("No compatible FPGA board connected to any serial port.");
                }
            }

            await taskCompletionSource.Task;
            return taskCompletionSource.Task.Result;
        }
    }
}
