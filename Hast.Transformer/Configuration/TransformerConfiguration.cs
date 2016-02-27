using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Common.Configuration
{
    public class TransformerConfiguration
    {
        /// <summary>
        /// Gets or sets the list of the member invokation instance counts, i.e. how many times a member can be invoked
        /// at a given time.
        /// </summary>
        public IList<MemberInvokationInstanceCountConfiguration> MemberInvokationInstanceCountConfigurations { get; set; }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
            MemberInvokationInstanceCountConfigurations = new List<MemberInvokationInstanceCountConfiguration>();
        }
    }


    public static class TransformerConfigurationExtensions
    {
        public static MemberInvokationInstanceCountConfiguration GetMaxInvokationInstanceCountConfigurationForMember(
            this TransformerConfiguration configuration, 
            string simpleMemberName)
        {
            var maxRecursionDepthConfig = configuration.MemberInvokationInstanceCountConfigurations
                .FirstOrDefault(config => simpleMemberName.StartsWith(config.MemberNamePrefix));

            if (maxRecursionDepthConfig != null) return maxRecursionDepthConfig;

            // Adding the configuration so if the object is modified it's saved in the TransformerConfiguration.
            var newConfiguration = new MemberInvokationInstanceCountConfiguration(simpleMemberName);
            configuration.MemberInvokationInstanceCountConfigurations.Add(newConfiguration);
            return newConfiguration;
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
