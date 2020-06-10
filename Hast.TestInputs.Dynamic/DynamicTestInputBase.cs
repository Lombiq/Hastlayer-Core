using Hast.Layer;
using Hast.Transformer.Abstractions.SimpleMemory;

namespace Hast.TestInputs.Dynamic
{
    public class DynamicTestInputBase
    {
        protected IHastlayer _hastlayer;
        protected IHardwareGenerationConfiguration _hardwareGenerationConfiguration;

        protected DynamicTestInputBase(IHastlayer hastlayer,
            IHardwareGenerationConfiguration hardwareGenerationConfiguration)
        {
            _hastlayer = hastlayer;
            _hardwareGenerationConfiguration = hardwareGenerationConfiguration;
        }


        protected SimpleMemory CreateMemory(int cellCount) =>
            _hastlayer.CreateMemory(_hardwareGenerationConfiguration, cellCount);
    }
}
