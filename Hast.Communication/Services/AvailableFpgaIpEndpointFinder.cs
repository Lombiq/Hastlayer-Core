using Hast.Communication.Constants;
using Hast.Communication.Helpers;
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
    public class AvailableFpgaIpEndpointFinder : IAvailableFpgaIpEndpointFinder
    {
        private const int AvailabilityCheckerTimeout = 1000;
        private const int BroadcastRetryCount = 2;
        private const string FpgaEndpointsCacheKey = "Hast.Communication.FpgaEndpoints";


        private readonly ICacheService _cacheService;
        private readonly IClock _clock;


        public AvailableFpgaIpEndpointFinder(ICacheService cacheService, IClock clock)
        {
            _cacheService = cacheService;
            _clock = clock;
        }


        public async Task<IPEndPoint> FindAnAvailableFpgaIpEndpoint()
        {
            var endpoints = _cacheService
                .Get(FpgaEndpointsCacheKey, () => GetFpgaEndpoints().Result).ToList();

            // Find an available endpoint that was lastly checked because it is probably still available.
            var availableEndpoint = endpoints
                .OrderByDescending(endpoint => endpoint.LastCheckedUtc)
                .FirstOrDefault(endpoint => endpoint.IsAvailable);

            if (availableEndpoint != null && await IsExistsAndAvailable(availableEndpoint))
                return UpdateCacheAndGetIpEndpoint(availableEndpoint, endpoints);

            // If there was no potentially available endpoint in the list let's check the longest unavailable FGPA if it is available now.
            if (availableEndpoint == null)
            {
                availableEndpoint = endpoints
                    .OrderBy(endpoint => endpoint.LastCheckedUtc)
                    .FirstOrDefault();

                if (availableEndpoint != null && await IsExistsAndAvailable(availableEndpoint))
                    return UpdateCacheAndGetIpEndpoint(availableEndpoint, endpoints);
            }

            // Otherwise the quickest way to find an available board is to broadcast a "Who is available" message again.
            endpoints = (await GetFpgaEndpoints()).ToList();

            availableEndpoint = endpoints
                .OrderByDescending(endpoint => endpoint.LastCheckedUtc)
                .FirstOrDefault(endpoint => endpoint.IsAvailable);

            if (availableEndpoint != null)
                return UpdateCacheAndGetIpEndpoint(availableEndpoint, endpoints);

            // If still no available FPGA then clear the cache and return null.
            _cacheService.Remove(FpgaEndpointsCacheKey);

            return null;
        }

        
        private IPEndPoint UpdateCacheAndGetIpEndpoint(FpgaEndpoint endpoint, IEnumerable<FpgaEndpoint> endpoints)
        {
            endpoint.IsAvailable = false;
            endpoint.LastCheckedUtc = _clock.UtcNow;
            
            _cacheService.Put(FpgaEndpointsCacheKey, endpoints);

            return endpoint.Endpoint;
        }

        private async Task<IEnumerable<FpgaEndpoint>> GetFpgaEndpoints()
        {
            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, CommunicationConstants.Ethernet.Ports.WhoIsAvailableRequest);
            var inputBuffer = new[] { (byte)CommandTypes.WhoIsAvailable };

            // We need retries because somehow the FPGA not always catches our request.
            var currentRetries = 0;
            var receiveResults = Enumerable.Empty<UdpReceiveResult>();
            while (currentRetries <= BroadcastRetryCount && !receiveResults.Any())
            {
                //udpClient.Send(inputBuffer, inputBuffer.Length, broadcastEndpoint);
                receiveResults = await EthernetCommunicationHelpers
                    .UdpSendAndReceiveAllAsync(inputBuffer, CommunicationConstants.Ethernet.Ports.WhoIsAvailableResponse, broadcastEndpoint, AvailabilityCheckerTimeout);

                currentRetries++;
            }

            return receiveResults.Select(result => CreateFpgaEndpoint(result.Buffer));
        }

        private async Task<bool> IsExistsAndAvailable(FpgaEndpoint fpgaEndpoint)
        {
            var availabilityCheckerEndpoint = new IPEndPoint(fpgaEndpoint.Endpoint.Address, CommunicationConstants.Ethernet.Ports.WhoIsAvailableRequest);
            var inputBuffer = new[] { (byte)CommandTypes.WhoIsAvailable };

            var receiveResult = await EthernetCommunicationHelpers
                .UdpSendAndReceiveAsync(inputBuffer, CommunicationConstants.Ethernet.Ports.WhoIsAvailableResponse, availabilityCheckerEndpoint, AvailabilityCheckerTimeout);

            if (receiveResult != default(UdpReceiveResult))
            {
                var createdEndpoint = CreateFpgaEndpoint(receiveResult.Buffer);

                if (createdEndpoint.IsAvailable && createdEndpoint.Endpoint.Equals(fpgaEndpoint.Endpoint))
                {
                    fpgaEndpoint.IsAvailable = true;
                    fpgaEndpoint.LastCheckedUtc = _clock.UtcNow;

                    return true;
                }

                return false;
            }

            return false;
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
        

        private class FpgaEndpoint
        {
            public bool IsAvailable { get; set; }

            public IPEndPoint Endpoint { get; set; }

            public DateTime LastCheckedUtc { get; set; }
        }
    }
}
