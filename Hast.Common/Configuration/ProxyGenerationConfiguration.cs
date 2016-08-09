using System.Collections.Generic;

namespace Hast.Common.Configuration
{
    public class ProxyGenerationConfiguration : IProxyGenerationConfiguration
    {
        /// <summary>
        /// Gets or sets a dictionary that can contain settings for non-default configuration options (like the name of 
        /// the communication channel).
        /// </summary>
        public IDictionary<string, object> CustomConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the communication channel used for communicating with the FPGA (eg. Ethernet).
        /// </summary>
        public string CommunicationChannelName { get; set; }

        /// <summary>
        /// Gets or sets whether the results coming from the hardware implementation should be validated against a
        /// software execution. If set to <c>true</c> then both a hardware and software invocation will happen and the
        /// result of the two compared. If there is a mismatch then an exception will be thrown.
        /// </summary>
        public bool ValidateHardwareResults { get; set; }


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
