using Hast.Communication.Exceptions;
using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    public class SerialPortCommunicationService : ICommunicationService
    {
        public Task Execute(SimpleMemory input, int methodId)
        {
            var sp = new SerialPort();
            var receiveByteSize = 0; // The incoming byte buffer size.
            var count = 0; // Just used to know when is the data ready.
            var returnValue = new List<byte>(); // The incoming buffer.

            // Initializing some serial port connection settings (Maybe different whith some fpga boards)
            sp.PortName = Constants.FpgaConstants.PortName;
            sp.BaudRate = 9600;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.WriteTimeout = 10000;

            try
            {
                // We try to open the serial port.
                sp.Open();
            }
            catch (Exception ex)
            {
                throw new SerialPortCommunicationException("Probably the FPGA board is not connected.", ex);
            }

            if (sp.IsOpen)
            {
                Debug.WriteLine("The port " + sp.PortName + " is ours.");
            }
            else
            {
                throw new SerialPortCommunicationException("The port " + sp.PortName + " is used by another app.");
            }

            //TODO: Here i need to write a code that sends the data to the FPGA.
            var length = input.Memory.Length;
            Debug.WriteLine("Data lengtg in bytes: " + length.ToString());
            var buffer = new byte[length + 9]; // Data message command + messageLength
            var lengthInBytes = Helpers.CommunicationHelpers.ConvertIntToByteArray(length);
            var methodIdInBytes = Helpers.CommunicationHelpers.ConvertIntToByteArray(methodId); // TODO: In future version store the methodID-s in database (automatically)

            // Here we put together the data stream.
            // Data message: |commanyType:1byte|messageLength:4byte|methodId:4byte|data
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
                buffer[i] = input.Memory[index];
                index++;
            }

            // Here the out buffer is ready
            //sp.Write(buffer, 0, length + 9);
            var j = 0;
            var byteBuf = new byte[1];

            while (j < length + 9)
            {
                byteBuf[0] = buffer[j];
                // Sending the data.
                sp.Write(byteBuf, 0, 1);
                j++;
            }

            var taskCompletionSource = new TaskCompletionSource<bool>();
            sp.DataReceived += (s, e) =>
            {
                // TODO: Here i need to write a code that receives the data.

                // When there are some incoming data then we read it from the serial port (this will be a byte that we receive)
                // The first byte will the size of the byte array that we must receive
                if (receiveByteSize == 0)// To setup the right receiving buffer size
                {
                    receiveByteSize = (byte)sp.ReadByte();
                    // The code below is just used for debud purposes.
                    var RXString = Convert.ToChar(receiveByteSize);
                    Debug.WriteLine("Incoming data size: " + RXString);
                }
                else
                {
                    // We put together the received data stream.
                    var receivedByte = (byte)sp.ReadByte();
                    returnValue.Add(receivedByte);
                    count++;
                }

                // Set the incoming data if all bytes are received. (Waiting for incoming data stream to complete.)
                if (receiveByteSize == count)
                {
                    var output = new SimpleMemory(receiveByteSize);
                    output.Memory = returnValue.ToArray();
                    taskCompletionSource.SetResult(true);
                }

            };

            // Send back the result.
            return taskCompletionSource.Task;
        }
    }
}
