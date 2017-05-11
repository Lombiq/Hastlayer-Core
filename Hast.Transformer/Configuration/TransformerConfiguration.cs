using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Common.Configuration
{
    public class TransformerConfiguration
    {
        private readonly ConcurrentDictionary<string, MemberInvocationInstanceCountConfiguration> _memberInvocationInstanceCountConfigurations =
            new ConcurrentDictionary<string, MemberInvocationInstanceCountConfiguration>();

        /// <summary>
        /// Gets or sets the list of the member invocation instance counts, i.e. how many times a member can be invoked
        /// at a given time.
        /// </summary>
        public IEnumerable<MemberInvocationInstanceCountConfiguration> MemberInvocationInstanceCountConfigurations
        {
            get { return _memberInvocationInstanceCountConfigurations.Values; }
        }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
        }


        public void AddMemberInvocationInstanceCountConfiguration(MemberInvocationInstanceCountConfiguration configuration)
        {
            _memberInvocationInstanceCountConfigurations
                .AddOrUpdate(configuration.MemberNamePrefix, configuration, (key, previousConfiguration) => configuration);
        }

        public MemberInvocationInstanceCountConfiguration GetMaxInvocationInstanceCountConfigurationForMember(
            string simpleMemberName)
        {
            var maxRecursionDepthConfig = MemberInvocationInstanceCountConfigurations
                //.ToArray() // To prevent "Collection was modified" exception.
                .FirstOrDefault(config => simpleMemberName.StartsWith(config.MemberNamePrefix));

            if (maxRecursionDepthConfig != null) return maxRecursionDepthConfig;

            // Adding the configuration so if the object is modified it's saved in the TransformerConfiguration.
            var newConfiguration = new MemberInvocationInstanceCountConfiguration(simpleMemberName);
            AddMemberInvocationInstanceCountConfiguration(newConfiguration);
            return newConfiguration;
        }
    }


    public static class HardwareGenerationConfigurationTransformerExtensions
    {
        public static TransformerConfiguration TransformerConfiguration(this IHardwareGenerationConfiguration hardwareConfiguration)
        {
            return hardwareConfiguration.GetOrAddCustomConfiguration<TransformerConfiguration>("Hast.Transformer.Configuration");
        }
    }
}
