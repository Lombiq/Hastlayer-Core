using Hast.Communication.Helpers;
using Hast.Transformer.SimpleMemory;
using Orchard.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    class EthernetCommunicationService : ICommunicationService
    {
        public ILogger Logger { get; set; }


        public EthernetCommunicationService()
        {
            Logger = NullLogger.Instance;
        }
        public async Task<Information> Execute(SimpleMemory simpleMemory, int memberId)
        {
            // The returned information object.
            var information = new Information();

            // Get the IP address of the FPGA board.
            var serverIPAddress = await CommunicationHelpers.GetFpgaIPAddress();
            // This is the port that the FPGA board is using.
            var serverPort = 7;
            Logger.Information("Socket: {0}:{1}", serverIPAddress, serverPort);

            var length = simpleMemory.Memory.Length;
            var outgoingDataBuffer = new byte[length + 9]; // The number 9 meaning is below (addig the message length and memberID to the output stream).

            var lengthInBytes = CommunicationHelpers.ConvertIntToByteArray(length); // Converting the length to byte array.
            var memberIdInBytes = CommunicationHelpers.ConvertIntToByteArray(memberId); // Converting the memberID to byte array.

            // Here we put together the outgoing data stream.
            outgoingDataBuffer[0] = 0; // commandType - not stored on FPGA - for future use.

            // Message length
            outgoingDataBuffer[1] = lengthInBytes[0];
            outgoingDataBuffer[2] = lengthInBytes[1];
            outgoingDataBuffer[3] = lengthInBytes[2];
            outgoingDataBuffer[4] = lengthInBytes[3];

            // Member ID
            outgoingDataBuffer[5] = memberIdInBytes[0];
            outgoingDataBuffer[6] = memberIdInBytes[1];
            outgoingDataBuffer[7] = memberIdInBytes[1];
            outgoingDataBuffer[8] = memberIdInBytes[3];

            var index = 0;
            for (int i = 9; i < length + 9; i++)
            {
                outgoingDataBuffer[i] = simpleMemory.Memory[index];
                index++;
            }

            try
            {
                // Initialize the connection.
                using (var client = new TcpClient(serverIPAddress, serverPort))
                {
                    // Get a client stream for reading and writing.
                    var stream = client.GetStream();

                    // Sending data to the FPGA board.
                    stream.Write(outgoingDataBuffer, 0, outgoingDataBuffer.Length);

                    // Buffer to store the response bytes.
                    var responseBuffer = new byte[length];

                    // Read the first batch of the TcpServer response bytes. Waiting for the response.
                    int bytes = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                    // Copy the results back to simpleMemory.Memory.
                    Buffer.BlockCopy(responseBuffer, 0, simpleMemory.Memory, 0, bytes);
                } // The client connection closes automatically.
            }
            catch (SocketException e) // Only catch socket exceptions.
            {
                Logger.Information("Socket exception: {0}", e);
            }

            return information;
        }
    }
}
