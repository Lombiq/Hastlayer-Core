using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Configuration
{
    public class TransformerConfiguration
    {
        public bool UseSimpleMemory { get; set; }


        public TransformerConfiguration()
        {
            UseSimpleMemory = true;
        }
    }


    public static class HardwareGenerationConfigurationExtensions
    {
        private const string ConfigKey = "Hast.Transformer.Configuration";


        public static TransformerConfiguration GetVhdlConfiguration(this IHardwareGenerationConfiguration hardwareConfiguration)
        {
            object config;

            if (hardwareConfiguration.CustomConfiguration.TryGetValue(ConfigKey, out config))
            {
                return (TransformerConfiguration)config;
            }

            return new TransformerConfiguration();
        }
    }
}
