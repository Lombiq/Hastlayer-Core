using Hast.Transformer.SimpleMemory;
using Orchard.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.Communication.Models;
using System.Diagnostics;
using System.Net;
using Hast.Communication.Constants;

namespace Hast.Communication.Services
{
    class EthernetCommunicationService : ICommunicationService
    {
        public ILogger Logger { get; set; }


        public EthernetCommunicationService()
        {
            Logger = NullLogger.Instance;
        }


        public async Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId)
        {
            // Stopwatch for measuring the total exection time.
            var stopWatch = Stopwatch.StartNew();
            var executionInformation = new HardwareExecutionInformation();

            // Get the IP address of the FPGA board.
            var fpgaIPAddress = await GetFpgaIPAddress();
            // This is the port that the FPGA board is using.
            var serverPort = 7;

            Logger.Information("Socket: {0}:{1}", fpgaIPAddress, serverPort);

            var length = simpleMemory.Memory.Length;
            var outputBuffer = new byte[length + 9];

            // Here we put together the data stream.

            // Execute Order 66.
            outputBuffer[0] = Convert.ToByte(CommandTypes.Execution);

            // Copying the input length, represented as bytes, to the output buffer.
            Array.Copy(BitConverter.GetBytes(length), 0, outputBuffer, 1, 4);

            // Copying the member ID, represented as bytes, to the output buffer.
            Array.Copy(BitConverter.GetBytes(memberId), 0, outputBuffer, 5, 4);

            for (int i = 0; i < length; i++)
            {
                outputBuffer[i + 9] = simpleMemory.Memory[i];
            }

            try
            {
                // Initialize the connection.
                using (var client = new TcpClient(fpgaIPAddress, serverPort))
                {
                    // Get a client stream for reading and writing.
                    var stream = client.GetStream();

                    // Sending data to the FPGA board.
                    stream.Write(outputBuffer, 0, outputBuffer.Length);

                    // Buffer to store the response bytes.
                    var responseBuffer = new byte[length];

                    // Read the first batch of the TcpServer response bytes. Waiting for the response.
                    int bytes = await stream.ReadAsync(responseBuffer, 0, responseBuffer.Length);
                    // Copy the results back to simpleMemory.Memory.
                    Buffer.BlockCopy(responseBuffer, 0, simpleMemory.Memory, 0, bytes);
                    // Why copy, why not just assign responseBuffer to simpleMemory.Memory?

                } // The client connection closes automatically.
            }
            catch (SocketException e) // Only catch socket exceptions.
            {
                Logger.Information("Socket exception: {0}", e);
            }

            stopWatch.Stop();
            Logger.Information("Full execution time: {0}ms", stopWatch.ElapsedMilliseconds);
            executionInformation.FullExecutionTimeMilliseconds = stopWatch.ElapsedMilliseconds;

            return executionInformation;
        }


        public static async Task<string> GetFpgaIPAddress()
        {
            // Get the IP address of the .NET side.
            var ipAddresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            // Not yet implemented. Please change this to the address of the FPGA board.
            return "192.168.0.107";
        }
    }
}
