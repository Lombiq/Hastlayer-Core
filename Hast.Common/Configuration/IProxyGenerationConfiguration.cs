using System.Collections.Generic;

namespace Hast.Common.Configuration
{
    public interface IProxyGenerationConfiguration
    {
        /// <summary>
        /// Gets a dictionary that can contain settings for non-default configuration options (like the name of the communication channel).
        /// </summary>
        IDictionary<string, object> CustomConfiguration { get; }
    }
}
