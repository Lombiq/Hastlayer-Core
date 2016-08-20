using System.Linq;
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
            return Create(HastlayerConfiguration.Default);
        }

        /// <summary>
        /// Instantiates a new <see cref="IHastlayer"/> implementation, configured for Xilinx FPGAs and the Xilinx FPGA
        /// development toolchain.
        /// </summary>
        /// <param name="configuration">Configuration for Hastalyer.</param>
        /// <returns>A newly created <see cref="IHastlayer"/> implementation.</returns>
        public static IHastlayer Create(IHastlayerConfiguration configuration)
        {
            configuration = new HastlayerConfiguration(configuration)
            {
                Extensions = new[]
                {
                    typeof(Transformer.Vhdl.Models.MemberIdTable).Assembly,
                    typeof(VivadoHardwareImplementationComposer).Assembly
                }.Union(configuration.Extensions),
                
            };

            return Hastlayer.Create(configuration);
        }
    }
}
