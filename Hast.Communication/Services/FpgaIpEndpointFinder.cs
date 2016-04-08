using Hast.Communication.Constants;
using Hast.Communication.Helpers;
using Hast.Communication.Models;
using Orchard.Caching;
using Orchard.Caching.Services;
using Orchard.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    public class FpgaIpEndpointFinder : IFpgaIpEndpointFinder
    {
        private const int AvailabilityCheckerTimeout = 1000;
        private const int BroadcastRetryCount = 2;
        private const string FpgaEndpointsCacheKey = "Hast.Communication.FpgaEndpoints";


        private readonly ICacheService _cacheService;
        private readonly IClock _clock;


        public FpgaIpEndpointFinder(ICacheService cacheService, IClock clock)
        {
            _cacheService = cacheService;
            _clock = clock;
        }


        public async Task<IEnumerable<IFpgaEndpoint>> FindFpgaEndpoints()
        {
            return _cacheService
                .Get(FpgaEndpointsCacheKey, () => GetFpgaEndpoints().Result);
        }
        

        private async Task<IEnumerable<FpgaEndpoint>> GetFpgaEndpoints()
        {
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, CommunicationConstants.Ethernet.Ports.WhoIsAvailableRequest);
            var inputBuffer = new[] { (byte)CommandTypes.WhoIsAvailable };

            // We need retries because somehow the FPGA doesn't always catch our request.
            var currentRetries = 0;
            var receiveResults = Enumerable.Empty<UdpReceiveResult>();
            while (currentRetries <= BroadcastRetryCount && !receiveResults.Any())
            {
                receiveResults = await EthernetCommunicationHelpers
                    .UdpSendAndReceiveAllAsync(inputBuffer, CommunicationConstants.Ethernet.Ports.WhoIsAvailableResponse, 
                        broadcastEndpoint, AvailabilityCheckerTimeout);

                currentRetries++;
            }

            return receiveResults.Select(result => CreateFpgaEndpoint(result.Buffer));
        }

        private FpgaEndpoint CreateFpgaEndpoint(byte[] answerBytes)
        {
            var isAvailable = Convert.ToBoolean(answerBytes[0]);
            var ipAddress = new IPAddress(answerBytes.Skip(1).Take(4).ToArray());
            var port = BitConverter.ToUInt16(answerBytes, 5);

            return new FpgaEndpoint
            {
                IsAvailable = isAvailable,
                Endpoint = new IPEndPoint(ipAddress, port),
                LastCheckedUtc = _clock.UtcNow
            };
        }
    }
}
