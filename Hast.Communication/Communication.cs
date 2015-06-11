using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication
{
    class Communication
    {
        enum commandType { data, command };
        SerialPort sp = new SerialPort();

        string portName = "COM4"; // This will be automated in next version

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
                sp.DataReceived += sp_DataReceived;
            }
            else
            {
                Debug.WriteLine("The port " + portName + " is used by another app.");
                sp.Close();
            }

            Debug.WriteLine("Ready, listening!");
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            char RXString = Convert.ToChar(sp.ReadByte());
            Console.WriteLine(RXString);

            if ('s' == RXString) // If the fpga is ready it sends a message
            {
                byte[] bytes = new byte[255];
                int count = 4;
                int i = 0;
                while (count < 255)
                {
                    bytes[count] = (byte)i;
                    count += 4;
                    i++;
                }


                sendData(bytes, commandType.data);
            }
        }

        private void sendData(byte[] bytes, commandType commandType)
        {
            switch (commandType)
            {
                case commandType.data:

                    // Now it is waiting for a data or command to receive
                    int length = bytes.Length;
                    Debug.WriteLine(length.ToString());
                    byte[] buffer = new byte[length + 5]; // Data message command + messageLength
                    MemoryStream stream = new MemoryStream();
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(length);
                    }
                    byte[] lengthInBytes = stream.ToArray();


                    // Data message: |commanyType:1byte|messageLength:4byte|
                    buffer[0] = (byte)commandType; //commanyType
                    buffer[1] = lengthInBytes[0]; // messageLength
                    buffer[2] = lengthInBytes[1]; // messageLength
                    buffer[3] = lengthInBytes[2]; // messageLength
                    buffer[4] = lengthInBytes[3]; // messageLength

                    // Manually
                    //buffer[1] = (byte)(length >> 24);
                    //buffer[2] = (byte)(length >> 16);
                    //buffer[3] = (byte)(length >> 8);
                    //buffer[4] = (byte)(length);
                    // Manually End

                    int index = 0;
                    for (int i = 5; i < length + 5; i++)
                    {
                        buffer[i] = bytes[index];
                        index++;
                    }

                    // Here the out buffer is ready
                    //sp.Write(buffer, 0, length + 5);
                    int j = 0;
                    byte[] byteBuf = new byte[1];

                    while (j < length + 5)
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
