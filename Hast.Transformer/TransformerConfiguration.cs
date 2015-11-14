
using System;
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
        /// The maximal depth of any call stack that can happen in the program.
        /// </summary>
        private int _maxCallStackDepth;
        public int MaxCallStackDepth
        {
            get { return _maxCallStackDepth; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException("The max call depth should be at least 1 otherwise methods can't invoke each other.");
                }
                _maxCallStackDepth = value;
            }
        }

        /// <summary>
        /// Determines whether to use the SimpleMemory memory model that maps a runtime-defined memory space to a byte
        /// array.
        /// </summary>
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
            MaxCallStackDepth = 1;
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
