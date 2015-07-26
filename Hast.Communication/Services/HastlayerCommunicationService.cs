using Hast.Transformer.SimpleMemory;
using Orchard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    /// <summary>
    /// The basic communication with the FPGA board.
    /// </summary>
    public interface IHastlayerCommunicationService : IDependency
    {
        Task<SimpleMemory> Execute(SimpleMemory input);
    }

    public class HastlayerCommunicationService : IHastlayerCommunicationService
    {
        public async Task<SimpleMemory> Execute(SimpleMemory input)
        {
            SerialPort sp = new SerialPort();
            int receiveByteSize = 0; // The incoming byte buffer size.
            int count = 0; // Just used to know when is the data ready.
            var returnValue = new List<byte>(); // The incoming buffer.

            //TODO: Initialize the connection.
            sp.PortName = Constants.FPGAConstants.PortName;
            sp.BaudRate = 9600;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.WriteTimeout = 10000;


            //TODO: Here i need to write a code, that sends the data to the FPGA.
            int length = input.Memory.Length;
            Debug.WriteLine(length.ToString());
            byte[] buffer = new byte[length + 9]; // Data message command + messageLength
            byte[] lengthInBytes = Helpers.CommunicationHelpers.ConvertIntToByteArray(length);
            byte[] methodIdInBytes = Helpers.CommunicationHelpers.ConvertIntToByteArray(0); // TODO: Set the method ID automatically


            // Data message: |commanyType:1byte|messageLength:4byte|methodId:4byte|data
            buffer[0] = 0; //commanyType - not stored on FPGA
            buffer[1] = lengthInBytes[0]; // messageLength
            buffer[2] = lengthInBytes[1]; // messageLength
            buffer[3] = lengthInBytes[2]; // messageLength
            buffer[4] = lengthInBytes[3]; // messageLength
            buffer[5] = methodIdInBytes[0];// MethodSelect
            buffer[6] = methodIdInBytes[1];// MethodSelect
            buffer[7] = methodIdInBytes[1];// MethodSelect
            buffer[8] = methodIdInBytes[3];// MethodSelect

            int index = 0;
            for (int i = 9; i < length + 9; i++)
            {
                buffer[i] = input.Memory[index];
                index++;
            }

            // Here the out buffer is ready
            //sp.Write(buffer, 0, length + 9);
            int j = 0;
            byte[] byteBuf = new byte[1];

            while (j < length + 9)
            {
                byteBuf[0] = buffer[j];
                // Sending the data.
                sp.Write(byteBuf, 0, 1);
                j++;
            }


            TaskCompletionSource<SimpleMemory> tcs = new TaskCompletionSource<SimpleMemory>();
            sp.DataReceived += (s, e) =>
            {
                // TODO: Here i need to write a code, that receives the data.

                // When there are some incoming data then we read it from the serial port (this will be a byte that we receive)
                // The first byte will the size of the byte array, that we must receive
                if (receiveByteSize == 0)// To setup the right receiving buffer size
                {
                    byte receivedByte = (byte)sp.ReadByte();
                    char RXString = Convert.ToChar(receivedByte);
                    Debug.WriteLine(RXString);
                }
                else
                {
                    byte receivedByte = (byte)sp.ReadByte();
                    returnValue.Add(receivedByte);
                    count++;
                }


                // Set the incoming data if all bytes are received.
                if (receiveByteSize == count)
                {
                    SimpleMemory output = new SimpleMemory(receiveByteSize);
                    output.Memory = returnValue.ToArray();
                    tcs.SetResult(output);
                }

            };
            // Waiting for incoming data stream to complete.
            await tcs.Task;

            // Send back the result.
            return new Task<SimpleMemory>();
        }
    }
}
