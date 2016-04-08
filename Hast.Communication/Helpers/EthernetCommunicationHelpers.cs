﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Hast.Communication.Helpers
{
    /// <summary>
    /// Helpers for communication operations via Ethernet.
    /// </summary>
    internal static class EthernetCommunicationHelpers
    {
        /// <summary>
        /// Sends an UDP datagram to an endpoint and receives answer if it arrives within a period of time.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="bindingPort">Port where the UDP clients (sender and receiver) will be bound.</param>
        /// <param name="targetEndpoint">Endpoint where the datagram needs to be sent.</param>
        /// <param name="receiveTimeoutMilliseconds">Timout within the answer datagram needs to arrive.</param>
        /// <returns>Result object containing UDP datagram received from the remote host. It is null if nothing has arrived.</returns>
        public static async Task<UdpReceiveResult> UdpSendAndReceiveAsync(byte[] datagram, int bindingPort, 
            IPEndPoint targetEndpoint, int receiveTimeoutMilliseconds)
        {
            return await UdpSendAndReceiveAnyAsync(client => client.ReceiveAsync(receiveTimeoutMilliseconds), 
                datagram, bindingPort, targetEndpoint, receiveTimeoutMilliseconds);
        }

        /// <summary>
        /// Sends an UDP datagram to an endpoint (possibly to a broadcast address) and receives every datagrams arriving within a period of time.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="bindingPort">Port where the UDP clients (sender and receiver) will be binded.</param>
        /// <param name="targetEndpoint">Endpoint where the datagram needs to be sent. Possibly it is a broadcast address.</param>
        /// <param name="receiveTimeoutMilliseconds">Timout within the answer datagram needs to arrive.</param>
        /// <returns>Result objects containing UDP datagram received from the remote host. It is empty if nothing has arrived.</returns>
        public static async Task<IEnumerable<UdpReceiveResult>> UdpSendAndReceiveAllAsync(byte[] datagram, int bindingPort, 
            IPEndPoint targetEndpoint, int receiveTimeoutMilliseconds)
        {
            return await UdpSendAndReceiveAnyAsync(client => client.ReceiveAllAsync(receiveTimeoutMilliseconds), 
                datagram, bindingPort, targetEndpoint, receiveTimeoutMilliseconds);
        }


        private static async Task<T> UdpSendAndReceiveAnyAsync<T>(Func<UdpClient, Task<T>> receiverTaskFactory, byte[] datagram, 
            int bindingPort, IPEndPoint targetEndpoint, int receiveTimeoutMilliseconds)
        {
            var udpEndpoint = new IPEndPoint(IPAddress.Any, bindingPort);

            // We need two UDP clients for sending and receiving datagrams. 
            // See: http://stackoverflow.com/questions/221783/udpclient-receive-right-after-send-does-not-work/222503#222503
            using (var receiverClient = CreateUdpClient(udpEndpoint))
            using (var senderClient = CreateUdpClient(udpEndpoint))
            {
                var receiveTask = receiverTaskFactory(receiverClient);
                var sendTask = senderClient.SendAsync(datagram, datagram.Length, targetEndpoint);

                await Task.WhenAll(receiveTask, sendTask);

                return receiveTask.Result;
            }
        }

        private static UdpClient CreateUdpClient(IPEndPoint bindingEndpoint)
        {
            var udpClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };

            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(bindingEndpoint);

            return udpClient;
        }
    }
}