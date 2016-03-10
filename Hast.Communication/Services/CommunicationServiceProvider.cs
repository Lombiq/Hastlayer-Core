using Hast.Communication.Constants;
using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Communication.Services
{
    /// <summary>
    /// Service for producing an <see cref="ICommunicationService"/> based on the given channel name.
    /// </summary>
    public interface ICommunicationServiceProvider : IDependency
    {
        /// <summary>
        /// Returns with the communication service based on the given channel name. If channel name is empty it returns with the default communication service.
        /// </summary>
        /// <param name="channelName">Name of the channel used to communicate.</param>
        /// <returns>Communication service based on the given channel name.</returns>
        ICommunicationService GetCommunicationService(string channelName = "");
    }


    public class CommunicationServiceProvider : ICommunicationServiceProvider
    {
        private const string DefaultChannelName = SerialCommunicationConstants.ChannelName;


        private readonly IEnumerable<ICommunicationService> _communicationServices;


        public CommunicationServiceProvider(IEnumerable<ICommunicationService> communicationServices)
        {
            _communicationServices = communicationServices;
        }


        public ICommunicationService GetCommunicationService(string channelName = "")
        {
            var communicationService = _communicationServices
                .FirstOrDefault(service => service.ChannelName == (string.IsNullOrEmpty(channelName) ? DefaultChannelName : channelName));

            if (communicationService == null)
                throw new InvalidOperationException("Communication service was not found with the given channel name.");

            return communicationService;
        }
    }
}
