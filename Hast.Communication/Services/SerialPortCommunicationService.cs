using Hast.Communication.Exceptions;
using Hast.Transformer.SimpleMemory;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using Hast.Communication.Helpers;
using Orchard.Logging;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : ICommunicationService
    {
        public ILogger Logger { get; set; }


        public SerialPortCommunicationService()
        {
            Logger = NullLogger.Instance;
        }


        public Task Execute(SimpleMemory simpleMemory, int memberId)
        {
            var serialPort = new SerialPort();

            // Initializing some serial port connection settings (Maybe different whith some fpga boards)
            var portName = Helpers.CommunicationHelpers.DetectSerialConnectionsPortName();

            serialPort.PortName = portName == null ? Constants.FpgaConstants.PortName : portName;
            serialPort.BaudRate = Constants.FpgaConstants.BaudRate;
            serialPort.Parity = Constants.FpgaConstants.SerialPortParity;
            serialPort.StopBits = Constants.FpgaConstants.SerialPortStopBits;
            serialPort.WriteTimeout = Constants.FpgaConstants.WriteTimeout;

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
                Debug.WriteLine("The port " + serialPort.PortName + " is ours.");
            }
            else
            {
                throw new SerialPortCommunicationException("Communication with the FPGA board through the serial port failed. The " + serialPort.PortName + " exists but it's used by another process.");
            }

            //TODO: Here i need to write a code that sends the data to the FPGA.
            var length = simpleMemory.Memory.Length;
            Debug.WriteLine("Data length in bytes: " + length.ToString());
            var buffer = new byte[length + 9]; // Data message command + messageLength
            var lengthInBytes = CommunicationHelpers.ConvertIntToByteArray(length);
            var memberIdInBytes = CommunicationHelpers.ConvertIntToByteArray(memberId);

            // Here we put together the data stream.
            // Data message: |commandType:1byte|messageLength:4byte|memberId:4byte|data
            buffer[0] = 0; //commandType - not stored on FPGA - deprecated

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

            // Here the out buffer is ready
            //sp.Write(buffer, 0, length + 9);
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
            var count = 0; // Just used to know when is the data ready.
            var returnValue = new byte[simpleMemory.Memory.Length]; // The incoming buffer
            var returnValueIndex = 0;
            var communicationType = '0'; // The 0 is the default value, the i is when we want to read something from the fpga, and the d if we want to read the processed data.
            var executionTime = new byte[4]; // In this variable is stored the execution time. (4Bytes)
            var executionTimeByteCounter = 0;

            // In this event we are receiving the userful data comfing from the FPGA board.
            serialPort.DataReceived += (s, e) =>
            {
                // When there are some incoming data then we read it from the serial port (this will be a byte that we receive)
                if (communicationType == '0')
                {
                    var temp = (byte)serialPort.ReadByte();
                    var RXString = Convert.ToChar(temp);
                    if (RXString == 'i') // i as information
                    {
                        communicationType = 'i';
                        serialPort.Write("s");
                    }
                    else
                    {
                        communicationType = 'd'; // as data
                    }
                }
                else if (communicationType == 'i')
                {
                    // We know that the incoming data's size will be 1 Byte.
                    executionTime[executionTimeByteCounter] = (byte)serialPort.ReadByte();
                    executionTimeByteCounter++; // We increment the byte counter to index the next incoming byte.
                    if (executionTimeByteCounter == 3) // If we receive the 4 bytes.
                    {
                        communicationType = 'd'; // We switch the communication type back to 'data'.
                        executionTimeByteCounter = 0;
                        // If the system architecture is little-endian (that is, little end first),
                        // reverse the byte array.
                        if (BitConverter.IsLittleEndian) Array.Reverse(executionTime); // Maybe this will be better in HelperMethods.. ByteArryToIntConversion()
                        // Log the information.
                        Logger.Information(BitConverter.ToInt32(executionTime, 0).ToString());
                    }
                    //Logger.Information(string.Format("Execution time: {0}", executionTime));
                    //serialPort.Write("s"); // Signal that we received the data. 'r'
                    //
                }
                else // If useful data is receiving. If the communicationType variable is equal with 'd' then this code will run.
                {
                    // The first byte will the size of the byte array that we must receive
                    if (messageSizeBytes == 0)// To setup the right receiving buffer size
                    {
                        // The first byte is the data size what we must receive.
                        messageSizeBytes = (byte)serialPort.ReadByte();
                        // The code below is just used for debug purposes.
                        var RXString = Convert.ToChar(messageSizeBytes);
                        Debug.WriteLine("Incoming data size: " + RXString.ToString());
                        serialPort.Write("s"); // Signal that we are ready to receive the data.
                    }
                    else
                    {
                        // We put together the received data stream.
                        var receivedByte = (byte)serialPort.ReadByte();
                        returnValue[returnValueIndex] = receivedByte;
                        returnValueIndex++;
                        count++;
                        serialPort.Write("r"); // Signal that we received all bytes.
                    }

                    // Set the incoming data if all bytes are received. (Waiting for incoming data stream to complete.)
                    if (messageSizeBytes == count)
                    {
                        //var output = new SimpleMemory(receiveByteSize);
                        simpleMemory.Memory = returnValue;
                        taskCompletionSource.SetResult(true);
                    }
                }
            };

            // Send back the result.
            return taskCompletionSource.Task;
        }
    }
}
