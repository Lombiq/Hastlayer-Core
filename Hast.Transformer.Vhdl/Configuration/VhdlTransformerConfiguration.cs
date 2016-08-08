using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Configuration
{
    public class VhdlTransformerConfiguration
    {
        public VhdlGenerationOptions VhdlGenerationOptions { get; set; } = new VhdlGenerationOptions();
    }


    public static class HardwareGenerationConfigurationTransformerExtensions
    {
        public static VhdlTransformerConfiguration VhdlTransformerConfiguration(this IHardwareGenerationConfiguration hardwareConfiguration)
        {
            return hardwareConfiguration.GetOrAddCustomConfiguration<VhdlTransformerConfiguration>("Hast.Transformer.Vhdl.Configuration");
        }
    }
}
