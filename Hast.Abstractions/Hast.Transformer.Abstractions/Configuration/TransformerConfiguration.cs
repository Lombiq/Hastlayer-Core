﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Hast.Common.Configuration;

namespace Hast.Transformer.Abstractions.Configuration
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
            // Since _memberInvocationInstanceCountConfigurations is a ConcurrentDictionary the order of its items is 
            // not necessarily the same on all machines or during all executions. Thus we need sorting so the 
            // transformation ID is deterministic (see DefaultTransformer in Hast.Transformer).
            get { return _memberInvocationInstanceCountConfigurations.Values.OrderBy(config => config.MemberNamePrefix); }
        }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; } = true;

        /// <summary>
        /// The lengths of arrays used in the code. Array sizes should be possible to determine statically and Hastlayer 
        /// can figure out what the compile-time size of an array is most of the time. Should this fail you can use 
        /// this to specify array lengths.
        /// 
        /// Key should be the full name of the array (<see cref="IHardwareGenerationConfiguration.HardwareEntryPointMemberFullNames"/>)
        /// and value should be the length. If you get exceptions due to arrays missing their sizes the exception will
        /// indicate the full array name too.
        /// </summary>
        public IDictionary<string, int> ArrayLengths { get; set; } = new Dictionary<string, int>();


        public void AddMemberInvocationInstanceCountConfiguration(MemberInvocationInstanceCountConfiguration configuration)
        {
            _memberInvocationInstanceCountConfigurations
                .AddOrUpdate(configuration.MemberNamePrefix, configuration, (key, previousConfiguration) => configuration);
        }

        public MemberInvocationInstanceCountConfiguration GetMaxInvocationInstanceCountConfigurationForMember(
            string simpleMemberName)
        {
            var maxRecursionDepthConfig = MemberInvocationInstanceCountConfigurations
                .Where(config => simpleMemberName.StartsWith(config.MemberNamePrefix))
                .OrderByDescending(config => config.MemberNamePrefix.Length)
                .FirstOrDefault();

            if (maxRecursionDepthConfig != null) return maxRecursionDepthConfig;

            // Adding the configuration so if the object is modified it's saved in the TransformerConfiguration.
            var newConfiguration = new MemberInvocationInstanceCountConfiguration(simpleMemberName);
            AddMemberInvocationInstanceCountConfiguration(newConfiguration);
            return newConfiguration;
        }

        public void AddLengthForMultipleArrays(int length, params string[] arrayNames)
        {
            for (int i = 0; i < arrayNames.Length; i++)
            {
                ArrayLengths.Add(arrayNames[i], length);
            }
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