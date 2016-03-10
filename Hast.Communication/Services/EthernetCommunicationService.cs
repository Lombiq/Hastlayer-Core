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
using Hast.Communication.Exceptions;
using Hast.Synthesis;

namespace Hast.Communication.Services
{
    public class EthernetCommunicationService : ICommunicationService
    {
        private readonly IDeviceDriver _deviceDriver;


        public ILogger Logger { get; set; }

        public string ChannelName
        {
            get
            {
                return EthernetCommunicationConstants.ChannelName; 
            }
        }


        public EthernetCommunicationService(IDeviceDriver deviceDriver)
        {
            _deviceDriver = deviceDriver;

            Logger = NullLogger.Instance;
        }


        public async Task<IHardwareExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId)
        {
            // Stopwatch for measuring the total exection time.
            var stopWatch = Stopwatch.StartNew();
            var executionInformation = new HardwareExecutionInformation();

            // Get the IP address of the FPGA board.
            var fpgaIPEndpoint = await GetFpgaIPEndPoint();
            
            Logger.Information("IP endpoint to communicate with via ethernet: {0}:{1}", fpgaIPEndpoint.Address, fpgaIPEndpoint.Port);

            try
            {
                // Initialize the connection.
                using (var client = new TcpClient())
                {
                    var success = await client.ConnectAsync(fpgaIPEndpoint, 3000);
                    if (!success)
                        throw new EthernetCommunicationException("Couldn't connect to FPGA before the timeout exceeded.");

                    var stream = client.GetStream();

                    var executionCommandTypeByte = new byte[1] { (byte)CommandTypes.Execution };
                    stream.Write(executionCommandTypeByte, 0, executionCommandTypeByte.Length);

                    var executionCommandTypeResponseByte = new byte[1];
                    await stream.ReadAsync(executionCommandTypeResponseByte, 0, executionCommandTypeResponseByte.Length);
                    
                    if (executionCommandTypeResponseByte[0] != EthernetCommunicationConstants.Signals.Ready)
                        throw new EthernetCommunicationException("Awaited a ready signal from the FPGA after the execution byte was sent but received the following byte instead: " + executionCommandTypeResponseByte[0]);

                    
                    var outputBytes = new byte[simpleMemory.Memory.Length + 8];
                    
                    // Copying the input length, represented as bytes, to the output buffer.
                    Array.Copy(BitConverter.GetBytes(simpleMemory.Memory.Length), 0, outputBytes, 0, 4);

                    // Copying the member ID, represented as bytes, to the output buffer.
                    Array.Copy(BitConverter.GetBytes(memberId), 0, outputBytes, 4, 4);

                    for (int i = 0; i < simpleMemory.Memory.Length; i++)
                    {
                        outputBytes[i + 8] = simpleMemory.Memory[i];
                    }

                    // Sending data to the FPGA board.
                    stream.Write(outputBytes, 0, outputBytes.Length);


                    // Buffer to store the execution time bytes.
                    var executionTimeBytes = new byte[8];

                    // Read the first batch of the TcpServer response bytes. Waiting for the response.
                    await stream.ReadAsync(executionTimeBytes, 0, executionTimeBytes.Length);

                    var executionTimeClockCycles = BitConverter.ToUInt64(executionTimeBytes, 0);

                    executionInformation.HardwareExecutionTimeMilliseconds =
                        1M / _deviceDriver.DeviceManifest.ClockFrequencyHz * 1000 * executionTimeClockCycles;
                    Logger.Information("Hardware execution took " + executionInformation.HardwareExecutionTimeMilliseconds + "ms.");

                    // Buffer to store the memory length bytes.
                    var outputByteCountBytes = new byte[4];
                    await stream.ReadAsync(outputByteCountBytes, 0, outputByteCountBytes.Length);

                    var outputByteCount = BitConverter.ToUInt32(outputByteCountBytes, 0);

                    Logger.Information("Incoming data size in bytes: {0}", outputByteCount);

                    // Buffer to store the memory bytes.
                    var memoryBuffer = new byte[outputByteCount];
                    await stream.ReadAsync(memoryBuffer, 0, (int)outputByteCount);

                    simpleMemory.Memory = memoryBuffer;
                }
            }
            catch (SocketException e)
            {
                throw new EthernetCommunicationException("An unexpected error was occurred during the Ethernet communication.", e);
            }

            stopWatch.Stop();
            Logger.Information("Full execution time: {0}ms", stopWatch.ElapsedMilliseconds);
            executionInformation.FullExecutionTimeMilliseconds = stopWatch.ElapsedMilliseconds;

            return executionInformation;
        }
        

        public static async Task<IPEndPoint> GetFpgaIPEndPoint()
        {
            var endpoint = new IPEndPoint(IPAddress.Parse("192.168.1.10"), 7);

            return endpoint;
        }
    }
}
