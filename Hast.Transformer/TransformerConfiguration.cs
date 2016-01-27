using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Common.Configuration
{
    public class TransformerConfiguration
    {
        /// <summary>
        /// Gets or sets the maximal degree of parallelism that will be attempted to build into the generated hardware
        /// when constructs suitable for hardware-level parallelisation are found.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

        /// <summary>
        /// Gets or sets the list of maximal recursion depth configurations for members. When using (even indirectly) 
        /// recursive calls between members set the maximal depth here.
        /// </summary>
        public IList<MemberMaxRecursionDepthConfiguration> MemberMaxRecursionDepthConfigurations { get; set; }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
            MemberMaxRecursionDepthConfigurations = new List<MemberMaxRecursionDepthConfiguration>();
        }


        public class MemberMaxRecursionDepthConfiguration
        {
            /// <summary>
            /// Gets or sets the prefix of the member's name. Use the same convention as with 
            /// <see cref="HardwareGenerationConfiguration.PublicHardwareMemberNamePrefixes"/>
            /// </summary>
            public string MemberNamePrefix { get; set; }

            private int _maxRecursionDepth;
            /// <summary>
            /// Gets or sets the maximal recursion depth of the member.
            /// </summary>
            public int MaxRecursionDepth
            {
                get { return _maxRecursionDepth; }
                set
                {
                    if (value < 1)
                    {
                        throw new ArgumentOutOfRangeException("The max recursion depth should be at least 1, otherwise the member wouldn't be transformed at all.");
                    }

                    _maxRecursionDepth = value;
                }
            }

        }
    }


    public static class TransformerConfigurationExtensions
    {
        public static int GetMaxRecursionDepthForMember(this TransformerConfiguration configuration, string memberName)
        {
            var maxRecursionDepthConfig = configuration.MemberMaxRecursionDepthConfigurations
                .FirstOrDefault(config => memberName.StartsWith(config.MemberNamePrefix));
            return maxRecursionDepthConfig != null ? maxRecursionDepthConfig.MaxRecursionDepth : 1;
        }
    }


    public static class HardwareGenerationConfigurationExtensions
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
