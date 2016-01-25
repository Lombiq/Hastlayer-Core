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
using Hast.Communication.Constants;
using Hast.Synthesis;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : ICommunicationService
    {
        private readonly IDeviceDriver _deviceDriver;

        public ILogger Logger { get; set; }


        public SerialPortCommunicationService(IDeviceDriver deviceDriver)
        {
            _deviceDriver = deviceDriver;

            Logger = NullLogger.Instance;
        }


        public async Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId)
        {
            // Stopwatch for measuring the exection time.
            var stopWatch = Stopwatch.StartNew();
            var executionInformation = new HardwareExecutionInformation();

            using (var serialPort = CreateSerialPort())
            {
                // Initializing some serial port connection settings (may be different whith some FPGA boards).
                // For detailed info on how the SerialPort class works see: https://social.msdn.microsoft.com/Forums/vstudio/en-US/e36193cd-a708-42b3-86b7-adff82b19e5e/how-does-serialport-handle-datareceived?forum=netfxbcl
                // Also we might consider this: http://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport

                serialPort.PortName = await GetFpgaPortName();

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

                // Execute Order 66.
                outputBuffer[0] = Convert.ToByte(SerialCommunicationConstants.CommandTypes.Execution);

                // Copying the input length, represented as bytes, to the output buffer.
                Array.Copy(BitConverter.GetBytes(length), 0, outputBuffer, 1, 4);

                // Copying the member ID, represented as bytes, to the output buffer.
                Array.Copy(BitConverter.GetBytes(memberId), 0, outputBuffer, 5, 4);

                for (int i = 0; i < length; i++)
                {
                    outputBuffer[i + 9] = simpleMemory.Memory[i];
                }

                // Sending the data.
                // Just using serialPort.Write() once with all the data would stop sending data after 16372 bytes so
                // we need to create batches. Since the FPGA receives data in the multiples of 4 bytes we use a batch
                // of 4 bytes. This seems to have no negative impact on performance compared to using
                // serialPort.Write() once.
                var maxBytesToSendAtOnce = 4;
                for (int i = 0; i < (int)Math.Ceiling(outputBuffer.Length / (decimal)maxBytesToSendAtOnce); i++)
                {
                    var remainingBytes = outputBuffer.Length - i * maxBytesToSendAtOnce;
                    var bytesToSend = remainingBytes > maxBytesToSendAtOnce ? maxBytesToSendAtOnce : remainingBytes;
                    serialPort.Write(outputBuffer, i * maxBytesToSendAtOnce, bytesToSend); 
                }


                // Processing the response.
                var taskCompletionSource = new TaskCompletionSource<bool>();
                var communicationState = SerialCommunicationConstants.CommunicationState.WaitForFirstResponse;
                var outputByteCountBytes = new byte[4];
                var outputByteCountByteCounter = 0;
                var outputByteCount = 0; // The incoming byte buffer size.
                var outputBytesReceivedCount = 0; // Just used to know when the data is ready.
                var outputBytes = new byte[0]; // The incoming buffer.
                var executionTimeBytes = new byte[8];
                var executionTimeByteCounter = 0;

                Action<byte, bool> processReceivedByte = (receivedByte, isLastOfBatch) =>
                    {
                        switch (communicationState)
                        {
                            case SerialCommunicationConstants.CommunicationState.WaitForFirstResponse:
                                if (receivedByte == SerialCommunicationConstants.Signals.Ping)
                                {
                                    communicationState = SerialCommunicationConstants.CommunicationState.ReceivingExecutionInformation;
                                    serialPort.Write(SerialCommunicationConstants.Signals.Ready);
                                }
                                else
                                {
                                    throw new SerialPortCommunicationException("Awaited a ping signal from the FPGA after it finished but received the following byte instead: " + receivedByte);
                                }
                                break;
                            case SerialCommunicationConstants.CommunicationState.ReceivingExecutionInformation:
                                executionTimeBytes[executionTimeByteCounter] = receivedByte;
                                executionTimeByteCounter++;
                                if (executionTimeByteCounter == 8)
                                {
                                    var executionTimeClockCycles = BitConverter.ToUInt64(executionTimeBytes, 0);

                                    executionInformation.HardwareExecutionTimeMilliseconds = (ulong)Math.Round(
                                        executionTimeClockCycles / (decimal)(_deviceDriver.DeviceManifest.ClockFrequencyHz / 1000));
                                    Logger.Information("Hardware execution took " + executionInformation.HardwareExecutionTimeMilliseconds + "ms.");

                                    communicationState = SerialCommunicationConstants.CommunicationState.ReceivingOutputByteCount;
                                    serialPort.Write(SerialCommunicationConstants.Signals.Ready);
                                }
                                break;
                            case SerialCommunicationConstants.CommunicationState.ReceivingOutputByteCount:
                                outputByteCountBytes[outputByteCountByteCounter] = receivedByte;
                                outputByteCountByteCounter++;

                                if (outputByteCountByteCounter == 4)
                                {
                                    outputByteCount = BitConverter.ToInt32(outputByteCountBytes, 0);

                                    // Since the output's size can differ from the input size for optimization reasons,
                                    // we take the explicit size into account.
                                    outputBytes = new byte[outputByteCount];

                                    Logger.Information("Incoming data size in bytes: {0}", outputByteCount);

                                    communicationState = SerialCommunicationConstants.CommunicationState.ReceivingOuput;
                                    serialPort.Write(SerialCommunicationConstants.Signals.Ready);
                                }
                                break;
                            case SerialCommunicationConstants.CommunicationState.ReceivingOuput:
                                outputBytes[outputBytesReceivedCount] = receivedByte;
                                outputBytesReceivedCount++;

                                if (outputByteCount == outputBytesReceivedCount)
                                {
                                    simpleMemory.Memory = outputBytes;

                                    // Serial communication can give more data than we actually await, so need to set this.
                                    communicationState = SerialCommunicationConstants.CommunicationState.Finished;
                                    serialPort.Write(SerialCommunicationConstants.Signals.Ready);

                                    taskCompletionSource.SetResult(true);
                                }
                                break;
                            default:
                                break;
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

                            if (communicationState == SerialCommunicationConstants.CommunicationState.Finished)
                            {
                                return;
                            }
                        }
                    }
                };

                await taskCompletionSource.Task;

                stopWatch.Stop();
                Logger.Information("Full execution time: {0}ms", stopWatch.ElapsedMilliseconds);
                executionInformation.FullExecutionTimeMilliseconds = stopWatch.ElapsedMilliseconds;

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

            // If no serial ports detected, then throw a SerialPortCommunicationException.
            if (ports.Length == 0) throw new SerialPortCommunicationException("No serial port detected (no serial ports are open).");

            using (var serialPort = CreateSerialPort())
            {
                var taskCompletionSource = new TaskCompletionSource<string>();

                serialPort.DataReceived += (sender, e) =>
                {
                    if (serialPort.ReadByte() == SerialCommunicationConstants.Signals.Ready)
                    {
                        taskCompletionSource.SetResult(serialPort.PortName);
                    }
                };

                for (int i = 0; i < ports.Length; i++)
                {
                    if (taskCompletionSource.Task.IsCompleted) break;

                    serialPort.PortName = ports[i];

                    try
                    {
                        serialPort.Open();
                        serialPort.Write(SerialCommunicationConstants.Signals.Echo);
                    }
                    catch (IOException) { }
                }

                // Waiting a maximum of 3s for a response from a port.
                if (!taskCompletionSource.Task.Wait(3000))
                {
                    throw new SerialPortCommunicationException("No compatible FPGA board connected to any serial port.");
                }

                return taskCompletionSource.Task.Result; 
            }
        }

        private static SerialPort CreateSerialPort()
        {
            var serialPort = new SerialPort();

            serialPort.BaudRate = SerialCommunicationConstants.BaudRate;
            serialPort.Parity = SerialCommunicationConstants.SerialPortParity;
            serialPort.StopBits = SerialCommunicationConstants.SerialPortStopBits;
            serialPort.WriteTimeout = SerialCommunicationConstants.WriteTimeoutInMilliseconds;

            return serialPort;
        }
    }
}
