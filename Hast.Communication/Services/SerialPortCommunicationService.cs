using Hast.Communication.Exceptions;
using Hast.Transformer.SimpleMemory;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : ICommunicationService
    {
        public Task Execute(SimpleMemory simpleMemory, int methodId)
        {
            var serialPort = new SerialPort();
            
            // Initializing some serial port connection settings (Maybe different whith some fpga boards)
            serialPort.PortName = Constants.FpgaConstants.PortName;
            serialPort.BaudRate = 9600;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.WriteTimeout = 10000;
            
            try
            {
                // We try to open the serial port.
                serialPort.Open();
            }
            catch (IOException ex)
            {
                throw new SerialPortCommunicationException("Communication with the FPGA board through the serial port failed.", ex);
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
            var lengthInBytes = Helpers.CommunicationHelpers.ConvertIntToByteArray(length);
            var methodIdInBytes = Helpers.CommunicationHelpers.ConvertIntToByteArray(methodId);

            // Here we put together the data stream.
            // Data message: |commandType:1byte|messageLength:4byte|methodId:4byte|data
            buffer[0] = 0; //commandType - not stored on FPGA - deprecated
            buffer[1] = lengthInBytes[0]; // messageLength
            buffer[2] = lengthInBytes[1]; // messageLength
            buffer[3] = lengthInBytes[2]; // messageLength
            buffer[4] = lengthInBytes[3]; // messageLength
            buffer[5] = methodIdInBytes[0];// MethodSelect
            buffer[6] = methodIdInBytes[1];// MethodSelect
            buffer[7] = methodIdInBytes[1];// MethodSelect
            buffer[8] = methodIdInBytes[3];// MethodSelect

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

            serialPort.DataReceived += (s, e) =>
            {
                // TODO: Here i need to write a code that receives the data.

                // When there are some incoming data then we read it from the serial port (this will be a byte that we receive)
                // The first byte will the size of the byte array that we must receive
                
                if (messageSizeBytes == 0)// To setup the right receiving buffer size
                {
                    // The first byte is the data size what we must receive.
                    messageSizeBytes = (byte)serialPort.ReadByte();
                    // The code below is just used for debud purposes.
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

            };
            
            // Send back the result.
            return taskCompletionSource.Task;
        }
    }
}
