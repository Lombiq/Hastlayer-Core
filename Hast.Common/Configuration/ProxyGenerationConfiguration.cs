using System.Collections.Generic;

namespace Hast.Common.Configuration
{
    public class ProxyGenerationConfiguration : IProxyGenerationConfiguration
    {
        /// <summary>
        /// Gets or sets a dictionary that can contain settings for non-default configuration options (like the name of the communication channel).
        /// </summary>
        public IDictionary<string, object> CustomConfiguration { get; set; }

        /// <summary>
        /// Communication channel used for communicating with the FPGA (eg. Ethernet).
        /// </summary>
        public string CommunicationChannelName { get; set; }


        private static IProxyGenerationConfiguration _default;
        public static IProxyGenerationConfiguration Default
        {
            get
            {
                if (_default == null)
                {
                    _default = new ProxyGenerationConfiguration
                    {
                        CommunicationChannelName = "Serial"
                    };
                }

                return _default;
            }
        }


        public ProxyGenerationConfiguration()
        {
            CustomConfiguration = new Dictionary<string, object>();
        }
    }
}
