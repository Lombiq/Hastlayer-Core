
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


        public static TransformerConfiguration GetTransformerConfiguration(this IHardwareGenerationConfiguration hardwareConfiguration)
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
