using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Common.Configuration
{
    public class TransformerConfiguration
    {
        /// <summary>
        /// Gets or sets the list of the member call instance counts, i.e. how many times a member can be called at a
        /// given time.
        /// </summary>
        public IList<MemberCallInstanceCountConfiguration> MemberCallInstanceCountConfigurations { get; set; }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
            MemberCallInstanceCountConfigurations = new List<MemberCallInstanceCountConfiguration>();
        }
    }


    public static class TransformerConfigurationExtensions
    {
        public static MemberCallInstanceCountConfiguration GetMaxCallInstanceCountConfigurationForMember(
            this TransformerConfiguration configuration, 
            string memberName)
        {
            var maxRecursionDepthConfig = configuration.MemberCallInstanceCountConfigurations
                .FirstOrDefault(config => memberName.StartsWith(config.MemberNamePrefix));

            if (maxRecursionDepthConfig != null) return maxRecursionDepthConfig;

            return new MemberCallInstanceCountConfiguration(memberName);
        }
    }


    public static class HardwareGenerationConfigurationTransformerExtensions
    {
        private const string ConfigKey = "Hast.Transformer.Configuration";


        public static TransformerConfiguration TransformerConfiguration(this IHardwareGenerationConfiguration hardwareConfiguration)
        {
            object config;

            if (hardwareConfiguration.CustomConfiguration.TryGetValue(ConfigKey, out config))
            {
                return (TransformerConfiguration)config;
            }

            return (TransformerConfiguration)(hardwareConfiguration.CustomConfiguration[ConfigKey] = new TransformerConfiguration());
        }
    }
}
