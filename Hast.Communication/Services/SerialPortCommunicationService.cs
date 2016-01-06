using Hast.Communication.Exceptions;
using Hast.Communication.Helpers;
using Hast.Transformer.SimpleMemory;
using Orchard.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : ICommunicationService
    {
        public ILogger Logger { get; set; }


        public SerialPortCommunicationService()
        {
            Logger = NullLogger.Instance;
        }


        public async Task<Information> Execute(SimpleMemory simpleMemory, int memberId)
        {
            var stopWatch = new Stopwatch(); // Measuring the exection time.
            stopWatch.Start(); // Start the measurement.
            var information = new Information();
            var serialPort = new SerialPort();

            // Initializing some serial port connection settings (may be different whith some FPGA boards).
            // For detailed info on how the SerialPort class works see: https://social.msdn.microsoft.com/Forums/vstudio/en-US/e36193cd-a708-42b3-86b7-adff82b19e5e/how-does-serialport-handle-datareceived?forum=netfxbcl

            serialPort.PortName = await CommunicationHelpers.GetFpgaPortName();
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
            var buffer = new byte[length + 9]; // Data message command + messageLength.
            var lengthInBytes = CommunicationHelpers.ConvertIntToByteArray(length);
            var memberIdInBytes = CommunicationHelpers.ConvertIntToByteArray(memberId);

            // Here we put together the data stream.
            buffer[0] = 0; // commandType - not stored on FPGA - for future use.

            // Message length
            buffer[1] = lengthInBytes[0];
            buffer[2] = lengthInBytes[1];
            buffer[3] = lengthInBytes[2];
            buffer[4] = lengthInBytes[3];

            // Member ID
            buffer[5] = memberIdInBytes[0];
            buffer[6] = memberIdInBytes[1];
            buffer[7] = memberIdInBytes[1];
            buffer[8] = memberIdInBytes[3];

            var index = 0;
            for (int i = 9; i < length + 9; i++)
            {
                buffer[i] = simpleMemory.Memory[index];
                index++;
            }

            // Here the out buffer is ready.
            var j = 0;
            var byteBuffer = new byte[1];

            while (j < length + 9)
            {
                byteBuffer[0] = buffer[j];
                // Sending the data.
                serialPort.Write(byteBuffer, 0, 1);
                j++;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();
            var messageSizeBytes = 0; // The incoming byte buffer size.
            var count = 0; // Just used to know when the data is ready.
            var returnValue = new byte[simpleMemory.Memory.Length]; // The incoming buffer.
            var returnValueIndex = 0;
            var communicationType = Constants.SerialCommunicationConstants.Signals.Default;
            var executionTime = new byte[4];
            var executionTimeByteCounter = 0;

            Action<byte> processReceivedByte = receivedByte =>
                {
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
                        executionTime[executionTimeByteCounter] = receivedByte;
                        executionTimeByteCounter++; // We increment the byte counter to index the next incoming byte.
                        if (executionTimeByteCounter == 4) // If we received the 4 bytes.
                        {
                            communicationType = Constants.SerialCommunicationConstants.Signals.Result; // We switch the communication type back to 'result'.
                            executionTimeByteCounter = 0;
                            // If the system architecture is little-endian (that is, little end first), reverse the byte array.
                            if (BitConverter.IsLittleEndian) Array.Reverse(executionTime);
                            // Log the information.
                            var executionTimeValue = BitConverter.ToInt32(executionTime, 0);
                            Logger.Information(executionTimeValue.ToString());
                            information.FpgaExecutionTime = executionTimeValue;
                            serialPort.Write(Constants.SerialCommunicationConstants.Signals.AllBytesReceived);
                        }
                    }
                    else // If the communicationType variable is equal with Constants.FpgaConstants.Signals.Data (d) then this code will run.
                    {
                        // The first byte will be the size of the byte array that we must receive.
                        if (messageSizeBytes == 0) // To setup the right receiving buffer size.
                        {
                            // The first byte is the data size that we must receive.
                            messageSizeBytes = receivedByte;
                            // The code below is just used for debug purposes.
                            var receivedCharacter = Convert.ToChar(messageSizeBytes);
                            Logger.Information("Incoming data size: {0}", receivedCharacter.ToString());
                            serialPort.Write(Constants.SerialCommunicationConstants.Signals.Ready); // Signal that we are ready to receive the data.
                        }
                        else
                        {
                            // We put together the received data stream.

                            returnValue[returnValueIndex] = receivedByte;
                            returnValueIndex++;
                            count++;
                            serialPort.Write(Constants.SerialCommunicationConstants.Signals.AllBytesReceived); // Signal that we received all bytes.
                        }

                        // Set the incoming data if all bytes are received (waiting for incoming data stream to complete).
                        if (messageSizeBytes == count)
                        {
                            simpleMemory.Memory = returnValue;
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
                        processReceivedByte(inputBuffer[i]);
                    }
                }
            };

            // Await the taskCompletionSource to complete.
            await taskCompletionSource.Task;
            stopWatch.Stop(); // Stop the exection time measurement.
            var fullExecutionTime = stopWatch.ElapsedMilliseconds;
            Logger.Information("Full execution time: {0}", fullExecutionTime.ToString());
            information.FullExecutionTime = fullExecutionTime;
            return information;
        }
    }
}
