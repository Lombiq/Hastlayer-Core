using System.Collections.Generic;

namespace Hast.Common.Configuration
{
    public interface IProxyGenerationConfiguration
    {
        /// <summary>
        /// Gets a dictionary that can contain settings for non-default configuration options.
        /// </summary>
        IDictionary<string, object> CustomConfiguration { get; }

        /// <summary>
        /// Communication channel used for communicating with the FPGA (eg. Ethernet).
        /// </summary>
        string CommunicationChannelName { get; }
    }
}
