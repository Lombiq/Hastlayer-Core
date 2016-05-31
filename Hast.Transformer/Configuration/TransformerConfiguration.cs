using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Common.Configuration
{
    public class TransformerConfiguration
    {
        /// <summary>
        /// Gets or sets the list of the member invocation instance counts, i.e. how many times a member can be invoked
        /// at a given time.
        /// </summary>
        public IList<MemberInvocationInstanceCountConfiguration> MemberInvocationInstanceCountConfigurations { get; set; }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
            MemberInvocationInstanceCountConfigurations = new List<MemberInvocationInstanceCountConfiguration>();
        }
    }


    public static class TransformerConfigurationExtensions
    {
        public static MemberInvocationInstanceCountConfiguration GetMaxInvocationInstanceCountConfigurationForMember(
            this TransformerConfiguration configuration, 
            string simpleMemberName)
        {
            var maxRecursionDepthConfig = configuration
                .MemberInvocationInstanceCountConfigurations
                .ToArray() // To prevent "Collection was modified" exception.
                .FirstOrDefault(config => simpleMemberName.StartsWith(config.MemberNamePrefix));

            if (maxRecursionDepthConfig != null) return maxRecursionDepthConfig;

            // Adding the configuration so if the object is modified it's saved in the TransformerConfiguration.
            var newConfiguration = new MemberInvocationInstanceCountConfiguration(simpleMemberName);
            configuration.MemberInvocationInstanceCountConfigurations.Add(newConfiguration);
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
