using Hast.Transformer.SimpleMemory;
using Orchard.Logging;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Hast.Common.Models;
using System.Net;
using Hast.Communication.Constants;
using Hast.Communication.Exceptions;
using Hast.Synthesis;
using Orchard.Caching;
using System.Text;
using System.Linq;

namespace Hast.Communication.Services
{
    public class EthernetCommunicationService : CommunicationServiceBase
    {
        private readonly IDeviceDriver _deviceDriver;
        private readonly IPingService _pingService;
        private readonly IAvailableFpgaIpEndpointFinder _availableFpgaFinder;


        public override string ChannelName
        {
            get
            {
                return CommunicationConstants.Ethernet.ChannelName; 
            }
        }


        public EthernetCommunicationService(
            IDeviceDriver deviceDriver, 
            IPingService pingService,
            IAvailableFpgaIpEndpointFinder availableFpgaFinder)
        {
            _deviceDriver = deviceDriver;
            _pingService = pingService;
            _availableFpgaFinder = availableFpgaFinder;
        }


        public override async Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId)
        {
            var executionInformation = BeginExecutionTimer();

            // Get the IP address of the FPGA board.
            var fpgaIPEndpoint = await _availableFpgaFinder.FindAnAvailableFpgaIpEndpoint();
            if (fpgaIPEndpoint == null)
                throw new EthernetCommunicationException("Couldn't find any available FPGA.");
            
            Logger.Information("IP endpoint to communicate with via ethernet: {0}:{1}", fpgaIPEndpoint.Address, fpgaIPEndpoint.Port);

            try
            {
                using (var client = new TcpClient())
                {
                    // Initialize the connection.
                    var success = await client.ConnectAsync(fpgaIPEndpoint, 3000);
                    if (!success)
                        throw new EthernetCommunicationException("Couldn't connect to FPGA before the timeout exceeded.");

                    var stream = client.GetStream();

                    // We send an execution signal to make the FPGA ready to receive the data stream.
                    var executionCommandTypeByte = new byte[1] { (byte)CommandTypes.Execution };
                    stream.Write(executionCommandTypeByte, 0, executionCommandTypeByte.Length);

                    var executionCommandTypeResponseByte = await GetBytesFromStream(stream, 1);
                    
                    if (executionCommandTypeResponseByte[0] != CommunicationConstants.Ethernet.Signals.Ready)
                        throw new EthernetCommunicationException("Awaited a ready signal from the FPGA after the execution byte was sent but received the following byte instead: " + executionCommandTypeResponseByte[0]);

                    // Here we put together the data stream.

                    // Copying the input length, represented as bytes, to the output buffer.
                    var outputBuffer = BitConverter.GetBytes(simpleMemory.Memory.Length)
                        // Copying the member ID, represented as bytes, to the output buffer.
                        .Append(BitConverter.GetBytes(memberId))
                        // Copying the simple memory.
                        .Append(simpleMemory.Memory);

                    // Sending data to the FPGA board.
                    stream.Write(outputBuffer, 0, outputBuffer.Length);


                    // Read the first batch of the TcpServer response bytes that will represent the execution time.
                    var executionTimeBytes = await GetBytesFromStream(stream, 8);

                    var executionTimeClockCycles = BitConverter.ToUInt64(executionTimeBytes, 0);

                    SetHardwareExecutionTime(executionInformation, _deviceDriver.DeviceManifest.ClockFrequencyHz, executionTimeClockCycles);

                    // Read the bytes representing the length of the simple memory.
                    var outputByteCountBytes = await GetBytesFromStream(stream, 4);

                    var outputByteCount = BitConverter.ToUInt32(outputByteCountBytes, 0);

                    Logger.Information("Incoming data size in bytes: {0}", outputByteCount);

                    // Finally read the memory itself.
                    var outputBytes = await GetBytesFromStream(stream, (int)outputByteCount);

                    simpleMemory.Memory = outputBytes;
                }
            }
            catch (SocketException e)
            {
                throw new EthernetCommunicationException("An unexpected error was occurred during the Ethernet communication.", e);
            }

            EndExecutionTimer(executionInformation);
            
            return executionInformation;
        }


        public static async Task<byte[]> GetBytesFromStream(NetworkStream stream, int length)
        {
            var outputBytes = new byte[length];
            await stream.ReadAsync(outputBytes, 0, outputBytes.Length);

            return outputBytes;
        }
    }
}
