using Hast.Transformer.SimpleMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Communication
{
    class Communication
    {
        // The command type - we are able to send two types of "data" 
        private enum commandType { data, command };

        private SerialPort sp = new SerialPort();
        private const string portName = "COM4"; // TODO: Automatikusan kiválasztani a megfelelő soros portot


        public Communication()
        {
            sp.PortName = portName;
            sp.BaudRate = 9600;
            sp.Parity = Parity.None;
            sp.StopBits = StopBits.One;
            sp.WriteTimeout = 10000;
        }

        public void Start()
        {
            try
            {
                sp.Open();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Probably the FPGA board is not connected. \nMessage: " + ex.Message);
            }

            if (sp.IsOpen)
            {
                Debug.WriteLine("The port " + portName + " is ours.");
                // We are listening, to incoming data
                sp.DataReceived += sp_DataReceived;
            }
            else
            {
                Debug.WriteLine("The port " + portName + " is used by another app.");
                sp.Close();
            }

            Debug.WriteLine("Ready, listening!");
        }

        // TODO: Megírni az adatok visszaérkezését kezelő programrészt - Execute metódus
        bool start = true;
        bool ready = true;
        byte[] returnValue = new byte[13];
        int count = 0;

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte receivedByte = (byte)sp.ReadByte();
            char RXString = Convert.ToChar(receivedByte);
            Debug.WriteLine(RXString);

            if ('s' == RXString) // If we receive a startup signal from the FPGA we execute the code
            {

                start = false;
                //sendData(ConvertIntToByteArray(1), commandType.data, 0);
            }
            else if ('r' == RXString)
            {
                ready = false;
            }
            else
            {
                Debug.WriteLine(receivedByte.ToString());
                returnValue[count] = receivedByte;
                count++;
            }


        }



        public SimpleMemory Execute(SimpleMemory input)
        {
            // TODO: Ellenőrizni, hogy a beérkező adat SimpleMemory típus -e
            while (start) { }
            Debug.WriteLine("Executing...");
            sendData(ConvertIntToByteArray(1), commandType.data, 1);         
            while (ready) { }
            SimpleMemory output = new SimpleMemory(13);
            output.Write4Bytes(0, returnValue);
            return output;
        }

        private byte[] ConvertIntToByteArray(int from)
        {
            MemoryStream stream = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(from);
            }
            return stream.ToArray();
        }

        // TODO: 
        private void sendData(byte[] bytes, commandType commandType, int methodId)
        {
            switch (commandType)
            {
                case commandType.data:

                    // Now it is waiting for a data or command to receive
                    int length = bytes.Length;
                    Debug.WriteLine(length.ToString());
                    byte[] buffer = new byte[length + 9]; // Data message command + messageLength
                    byte[] lengthInBytes = ConvertIntToByteArray(length);
                    byte[] methodIdInBytes = ConvertIntToByteArray(methodId);


                    // Data message: |commanyType:1byte|messageLength:4byte|methodId:4byte|data
                    buffer[0] = (byte)commandType; //commanyType - not stored on FPGA
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
                        buffer[i] = bytes[index];
                        index++;
                    }

                    // Here the out buffer is ready
                    //sp.Write(buffer, 0, length + 9);
                    int j = 0;
                    byte[] byteBuf = new byte[1];

                    while (j < length + 9)
                    {
                        byteBuf[0] = buffer[j];
                        sp.Write(byteBuf, 0, 1);
                        j++;
                        //Thread.Sleep(20);
                    }

                    break;
                case commandType.command:
                    sp.Write(new byte[] { (byte)commandType }, 0, 1);
                    break;
                default:
                    break;
            }
        }
    }
}
