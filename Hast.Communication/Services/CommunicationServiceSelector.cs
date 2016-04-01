using Hast.Communication.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Communication.Services
{
    public class CommunicationServiceSelector : ICommunicationServiceSelector
    {
        private readonly IEnumerable<ICommunicationService> _communicationServices;


        public CommunicationServiceSelector(IEnumerable<ICommunicationService> communicationServices)
        {
            _communicationServices = communicationServices;
        }


        public ICommunicationService GetCommunicationService(string channelName = "")
        {
            var communicationService = _communicationServices
                .FirstOrDefault(service => 
                    service.ChannelName == (string.IsNullOrEmpty(channelName) ? CommunicationConstants.DefaultChannelName : channelName));

            if (communicationService == null)
                throw new InvalidOperationException("Communication service was not found with the given channel name.");

            return communicationService;
        }
    }
}
