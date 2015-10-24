using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Layer;

namespace Hast.Xilinx
{
    public static class HastlayerFactory
    {
        /// <summary>
        /// Instantiates a new <see cref="IHastlayer"/> implementation, configured for Xilinx FPGAs and the Xilinx FPGA
        /// development toolchain.
        /// </summary>
        /// <returns>A newly created <see cref="IHastlayer"/> implementation.</returns>
        public static IHastlayer Create()
        {
            return Create(Enumerable.Empty<Assembly>());
        }

        /// <summary>
        /// Instantiates a new <see cref="IHastlayer"/> implementation, configured for Xilinx FPGAs and the Xilinx FPGA
        /// development toolchain.
        /// </summary>
        /// <param name="extensions">
        /// Extensions that can provide implementations for Hastlayer services or hook into the hardware generation 
        /// pipeline.
        /// </param>
        /// <returns>A newly created <see cref="IHastlayer"/> implementation.</returns>
        public static IHastlayer Create(IEnumerable<Assembly> extensions)
        {
            extensions = new[]
                {
                    typeof(Hast.Transformer.Vhdl.InterfaceMethodDefinition).Assembly,
                    typeof(Hast.Xilinx.XilinxHardwareRepresentationComposer).Assembly
                }.Union(extensions);

            return Hastlayer.Create(extensions);
        }
    }
}
