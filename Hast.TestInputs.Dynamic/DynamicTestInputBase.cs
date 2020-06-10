using Hast.Layer;
using Hast.Transformer.Abstractions.SimpleMemory;

namespace Hast.TestInputs.Dynamic
{
    public class DynamicTestInputBase
    {
        public IHastlayer Hastlayer { get; set; }
        public IHardwareGenerationConfiguration HardwareGenerationConfiguration { get; set; }


        protected SimpleMemory CreateMemory(int cellCount) =>
            Hastlayer.CreateMemory(HardwareGenerationConfiguration, cellCount);
    }
}
