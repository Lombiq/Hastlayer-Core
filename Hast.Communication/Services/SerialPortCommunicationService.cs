using Hast.Communication.Exceptions;
using Hast.Transformer.SimpleMemory;
using Orchard.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using Hast.Communication.Models;
using Hast.Common.Models;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : ICommunicationService
    {
        private readonly ISerialPortNameCache _serialPortNameCache;

        public ILogger Logger { get; set; }


        public SerialPortCommunicationService(ISerialPortNameCache serialPortNameCache)
        {
            _serialPortNameCache = serialPortNameCache;

            Logger = NullLogger.Instance;
        }


        public async Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId)
        {
            // Stopwatch for measuring the exection time.
            var stopWatch = Stopwatch.StartNew();
            var executionInformation = new HardwareExecutionInformation();

            using (var serialPort = new SerialPort())
            {
                // Initializing some serial port connection settings (may be different whith some FPGA boards).
                // For detailed info on how the SerialPort class works see: https://social.msdn.microsoft.com/Forums/vstudio/en-US/e36193cd-a708-42b3-86b7-adff82b19e5e/how-does-serialport-handle-datareceived?forum=netfxbcl

                if (string.IsNullOrEmpty(_serialPortNameCache.PortName))
                {
                    _serialPortNameCache.PortName = await GetFpgaPortName();
                }
                serialPort.PortName = _serialPortNameCache.PortName;
                serialPort.BaudRate = Constants.SerialCommunicationConstants.BaudRate;
                serialPort.Parity = Constants.SerialCommunicationConstants.SerialPortParity;
                serialPort.StopBits = Constants.SerialCommunicationConstants.SerialPortStopBits;
                serialPort.WriteTimeout = Constants.SerialCommunicationConstants.WriteTimeoutInMilliseconds;

                try
                {
                    // We try to open the serial port.
                    serialPort.Open();
                }
                catch (IOException ex)
                {
                    throw new SerialPortCommunicationException("Communication with the FPGA board through the serial port failed. Probably the FPGA board is not connected.", ex);
                }

                if (serialPort.IsOpen)
                {
                    Logger.Information("The port {0} is ours.", serialPort.PortName);
                }
                else
                {
                    throw new SerialPortCommunicationException("Communication with the FPGA board through the serial port failed. The " + serialPort.PortName + " exists but it's used by another process.");
                }

                var length = simpleMemory.Memory.Length;
                var outputBuffer = new byte[length + 9];

                // Here we put together the data stream.
                outputBuffer[0] = 0; // commandType - not used on the FPGA - for future use.

                // Copying the message length, represented as bytes, to the output buffer.
                Array.Copy(BitConverter.GetBytes(length), 0, outputBuffer, 1, 4);

                // Copying the member ID, represented as bytes, to the output buffer.
                Array.Copy(BitConverter.GetBytes(memberId), 0, outputBuffer, 5, 4);

                for (int i = 0; i < length; i++)
                {
                    outputBuffer[i + 9] = simpleMemory.Memory[i];
                }

                // Sending the data.
                serialPort.Write(outputBuffer, 0, outputBuffer.Length);

                var taskCompletionSource = new TaskCompletionSource<bool>();
                var communicationType = Constants.SerialCommunicationConstants.Signals.Default;
                var messageBytesCountBytes = new byte[4];
                var messageBytesCountByteCounter = 0;
                var messageBytesCount = 0; // The incoming byte buffer size.
                var messageBytesReceivedCount = 0; // Just used to know when the data is ready.
                var receivedValueBytes = new byte[0]; // The incoming buffer.
                var executionTimeClockCyclesBytes = new byte[4];
                var executionTimeByteCounter = 0;

                Action<byte, bool> processReceivedByte = (receivedByte, isLastOfBatch) =>
                    {
                        // Serial communication can give more data than we actually await, so need to check this.
                        if (taskCompletionSource.Task.IsCompleted) return;

                        // When there is some incoming data then we read it from the serial port (this will be a byte that we receive).
                        if (communicationType == Constants.SerialCommunicationConstants.Signals.Default)
                        {
                            var receivedCharacter = Convert.ToChar(receivedByte);
                            if (receivedCharacter == Constants.SerialCommunicationConstants.Signals.Information)
                            {
                                communicationType = Constants.SerialCommunicationConstants.Signals.Information;
                                serialPort.Write(Constants.SerialCommunicationConstants.Signals.Ready);
                            }
                            else
                            {
                                communicationType = Constants.SerialCommunicationConstants.Signals.Result;
                            }
                        }
                        else if (communicationType == Constants.SerialCommunicationConstants.Signals.Information)
                        {
                            // We know that the incoming data's size will be 4 bytes.
                            executionTimeClockCyclesBytes[executionTimeByteCounter] = receivedByte;
                            executionTimeByteCounter++; // We increment the byte counter to index the next incoming byte.
                            if (executionTimeByteCounter == 4) // If we received the 4 bytes.
                            {
                                // We switch the communication type back to 'result'.
                                communicationType = Constants.SerialCommunicationConstants.Signals.Result;
                                executionTimeByteCounter = 0;

                                var executionTimeClockCycles = BitConverter.ToUInt32(executionTimeClockCyclesBytes, 0);

                                // Hard-coding the divisor for now: the FPGA runs at 100Mhz so one clock cycle is 10ns.
                                executionInformation.HardwareExecutionTimeMilliseconds = executionTimeClockCycles / 100000;
                                Logger.Information("Hardware execution took " + executionInformation.HardwareExecutionTimeMilliseconds + "ms.");
                                
                                serialPort.Write(Constants.SerialCommunicationConstants.Signals.AllBytesReceived);
                            }
                        }
                        else // If the communicationType variable is equal to Constants.FpgaConstants.Signals.Data (d) then this code will run.
                        {
                            // The first bytes will be the size of the byte array that we must receive.
                            if (messageBytesCount == 0)
                            {
                                messageBytesCountBytes[messageBytesCountByteCounter] = receivedByte;
                                messageBytesCountByteCounter++;

                                if (messageBytesCountByteCounter == 4)
                                {
                                    messageBytesCount = BitConverter.ToInt32(messageBytesCountBytes, 0);

                                    // Since the return value's size can differ from the input size for optimization reasons,
                                    // we take the explicit size into account.
                                    receivedValueBytes = new byte[messageBytesCount];

                                    Logger.Information("Incoming data size: {0}", Convert.ToChar(messageBytesCount).ToString());

                                    // Signal that we are ready to receive the data.
                                    serialPort.Write(Constants.SerialCommunicationConstants.Signals.Ready); 
                                }
                            }
                            else
                            {
                                // We put together the received data stream.

                                receivedValueBytes[messageBytesReceivedCount] = receivedByte;
                                messageBytesReceivedCount++;
                                if (isLastOfBatch)
                                {
                                    serialPort.Write(Constants.SerialCommunicationConstants.Signals.Ready); 
                                }
                            }

                            // Set the incoming data if all bytes are received (waiting for incoming data stream to complete).
                            if (messageBytesCount > 0 && messageBytesCount == messageBytesReceivedCount)
                            {
                                // Signal that we received all bytes.
                                serialPort.Write(Constants.SerialCommunicationConstants.Signals.AllBytesReceived);

                                simpleMemory.Memory = receivedValueBytes;
                                taskCompletionSource.SetResult(true);

                            }
                        }
                    };

                // In this event we are receiving the useful data coming from the FPGA board.
                serialPort.DataReceived += (s, e) =>
                {
                    if (e.EventType == SerialData.Chars)
                    {
                        var inputBuffer = new byte[serialPort.BytesToRead];
                        serialPort.Read(inputBuffer, 0, inputBuffer.Length);

                        for (int i = 0; i < inputBuffer.Length; i++)
                        {
                            processReceivedByte(inputBuffer[i], i == inputBuffer.Length - 1);

                            if (taskCompletionSource.Task.IsCompleted) return;
                        }
                    }
                };

                // Await the taskCompletionSource to complete.
                await taskCompletionSource.Task;

                stopWatch.Stop(); // Stop the exection time measurement.
                var fullExecutionTime = stopWatch.ElapsedMilliseconds;
                Logger.Information("Full execution time (in milliseconds): {0}", fullExecutionTime.ToString());
                executionInformation.FullExecutionTimeMilliseconds = fullExecutionTime;

                return executionInformation; 
            }
        }


        /// <summary>
        /// Helper method used for the detection of the connected FPGA board.
        /// </summary>
        /// <returns>The serial port name where the FPGA board is connected to.</returns>
        private static async Task<string> GetFpgaPortName()
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
