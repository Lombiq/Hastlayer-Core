﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Extensibility.Pipeline;
using Hast.Communication.Constants;
using Hast.Communication.Exceptions;
using Hast.Communication.Extensibility.Pipeline;
using Hast.Communication.Models;
using Hast.Layer;
using Hast.Transformer.Abstractions.SimpleMemory;
using Orchard.Logging;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : CommunicationServiceBase
    {
        private readonly IDevicePoolPopulator _devicePoolPopulator;
        private readonly IDevicePoolManager _devicePoolManager;
        private readonly IEnumerable<ISerialPortConfigurator> _serialPortConfigurators;

        public override string ChannelName
        {
            get
            {
                return CommunicationConstants.Serial.ChannelName;
            }
        }


        public SerialPortCommunicationService(
            IDevicePoolPopulator devicePoolPopulator,
            IDevicePoolManager devicePoolManager,
            IEnumerable<ISerialPortConfigurator> serialPortConfigurators)
        {
            _devicePoolPopulator = devicePoolPopulator;
            _devicePoolManager = devicePoolManager;
            _serialPortConfigurators = serialPortConfigurators;
        }


        public override async Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory,
            int memberId,
            IHardwareExecutionContext executionContext)
        {
            _devicePoolPopulator.PopulateDevicePoolIfNew(async () =>
                {
                    var portNames = await GetFpgaPortNames(executionContext);
                    return portNames.Select(portName => new Device { Identifier = portName });
                });

            using (var device = await _devicePoolManager.ReserveDevice())
            {
                var context = BeginExecution();

                // Initializing some serial port connection settings (may be different with some FPGA boards).
                // For detailed info on how the SerialPort class works see: https://social.msdn.microsoft.com/Forums/vstudio/en-US/e36193cd-a708-42b3-86b7-adff82b19e5e/how-does-serialport-handle-datareceived?forum=netfxbcl
                // Also we might consider this: http://www.sparxeng.com/blog/software/must-use-net-system-io-ports-serialport

                using (var serialPort = CreateSerialPort(executionContext))
                {
                    serialPort.PortName = device.Identifier;

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

                    // Here we put together the data stream.

                    // Execute Order 66.
                    var outputBuffer = new byte[] { (byte)CommandTypes.Execution }
                        // Copying the input length, represented as bytes, to the output buffer.
                        .Append(BitConverter.GetBytes(simpleMemory.Memory.Length))
                        // Copying the member ID, represented as bytes, to the output buffer.
                        .Append(BitConverter.GetBytes(memberId))
                        // Copying the simple memory.
                        .Append(simpleMemory.Memory);

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
                    var communicationState = CommunicationConstants.Serial.CommunicationState.WaitForFirstResponse;
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
                                case CommunicationConstants.Serial.CommunicationState.WaitForFirstResponse:
                                    if (receivedByte == CommunicationConstants.Serial.Signals.Ping)
                                    {
                                        communicationState = CommunicationConstants.Serial.CommunicationState.ReceivingExecutionInformation;
                                        serialPort.Write(CommunicationConstants.Serial.Signals.Ready);
                                    }
                                    else
                                    {
                                        throw new SerialPortCommunicationException("Awaited a ping signal from the FPGA after it finished but received the following byte instead: " + receivedByte);
                                    }
                                    break;
                                case CommunicationConstants.Serial.CommunicationState.ReceivingExecutionInformation:
                                    executionTimeBytes[executionTimeByteCounter] = receivedByte;
                                    executionTimeByteCounter++;
                                    if (executionTimeByteCounter == 8)
                                    {
                                        var executionTimeClockCycles = BitConverter.ToUInt64(executionTimeBytes, 0);

                                        SetHardwareExecutionTime(context, executionContext, executionTimeClockCycles);

                                        communicationState = CommunicationConstants.Serial.CommunicationState.ReceivingOutputByteCount;
                                        serialPort.Write(CommunicationConstants.Serial.Signals.Ready);
                                    }
                                    break;
                                case CommunicationConstants.Serial.CommunicationState.ReceivingOutputByteCount:
                                    outputByteCountBytes[outputByteCountByteCounter] = receivedByte;
                                    outputByteCountByteCounter++;

                                    if (outputByteCountByteCounter == 4)
                                    {
                                        outputByteCount = BitConverter.ToInt32(outputByteCountBytes, 0);

                                        // Since the output's size can differ from the input size for optimization reasons,
                                        // we take the explicit size into account.
                                        outputBytes = new byte[outputByteCount];

                                        Logger.Information("Incoming data size in bytes: {0}", outputByteCount);

                                        communicationState = CommunicationConstants.Serial.CommunicationState.ReceivingOuput;
                                        serialPort.Write(CommunicationConstants.Serial.Signals.Ready);
                                    }
                                    break;
                                case CommunicationConstants.Serial.CommunicationState.ReceivingOuput:
                                    outputBytes[outputBytesReceivedCount] = receivedByte;
                                    outputBytesReceivedCount++;

                                    if (outputByteCount == outputBytesReceivedCount)
                                    {
                                        simpleMemory.Memory = outputBytes;

                                        // Serial communication can give more data than we actually await, so need to set this.
                                        communicationState = CommunicationConstants.Serial.CommunicationState.Finished;
                                        serialPort.Write(CommunicationConstants.Serial.Signals.Ready);

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

                                if (communicationState == CommunicationConstants.Serial.CommunicationState.Finished)
                                {
                                    return;
                                }
                            }
                        }
                    };

                    await taskCompletionSource.Task;

                    EndExecution(context);

                    return context.HardwareExecutionInformation;
                }
            }
        }


        /// <summary>
        /// Detects serial-connected compatible FPGA boards.
        /// </summary>
        /// <returns>The serial port name where the FPGA board is connected to.</returns>
        private async Task<IEnumerable<string>> GetFpgaPortNames(IHardwareExecutionContext executionContext)
        {
            // Get all available serial ports in the system.
            var ports = SerialPort.GetPortNames();

            // If no serial ports were detected, then we can't do anything else.
            if (ports.Length == 0)
            {
                throw new SerialPortCommunicationException("No serial port detected (no serial ports are open).");
            }

            var fpgaPortNames = new ConcurrentBag<string>();
            var serialPortPingingTasks = new Task[ports.Length];

            for (int i = 0; i < ports.Length; i++)
            {
                serialPortPingingTasks[i] = Task.Factory.StartNew(portNameObject =>
                    {
                        using (var serialPort = CreateSerialPort(executionContext))
                        {
                            var taskCompletionSource = new TaskCompletionSource<bool>();

                            serialPort.DataReceived += (sender, e) =>
                            {
                                if (serialPort.ReadByte() == CommunicationConstants.Serial.Signals.Ready)
                                {
                                    fpgaPortNames.Add(serialPort.PortName);
                                    taskCompletionSource.SetResult(true);
                                }
                            };

                            serialPort.PortName = (string)portNameObject;

                            try
                            {
                                serialPort.Open();
                                serialPort.Write(CommandTypes.WhoIsAvailable);
                            }
                            catch (IOException) { }

                            // Waiting a maximum of 3s for a response from the port.
                            taskCompletionSource.Task.Wait(3000);
                        }
                    }, ports[i]);
            }

            await Task.WhenAll(serialPortPingingTasks);

            if (!fpgaPortNames.Any())
            {
                throw new SerialPortCommunicationException("No compatible FPGA board connected to any serial port.");
            }

            return fpgaPortNames;
        }

        private SerialPort CreateSerialPort(IHardwareExecutionContext executionContext)
        {
            var serialPort = new SerialPort();

            serialPort.BaudRate = CommunicationConstants.Serial.DefaultBaudRate;
            serialPort.Parity = CommunicationConstants.Serial.DefaultParity;
            serialPort.StopBits = CommunicationConstants.Serial.DefaultStopBits;
            serialPort.WriteTimeout = CommunicationConstants.Serial.DefaultWriteTimeoutMilliseconds;

            _serialPortConfigurators.InvokePipelineSteps(step => step.ConfigureSerialPort(serialPort, executionContext));

            return serialPort;
        }
    }
}
