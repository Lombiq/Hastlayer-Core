using System.Collections.Generic;

namespace Hast.Common.Configuration
{
    public class ProxyGenerationConfiguration : IProxyGenerationConfiguration
    {
        /// <summary>
        /// Gets or sets a dictionary that can contain settings for non-default configuration options (like the name of the communication channel).
        /// </summary>
        public IDictionary<string, object> CustomConfiguration { get; set; }
        

        public ProxyGenerationConfiguration()
        {
            CustomConfiguration = new Dictionary<string, object>();
        }
    }
}
