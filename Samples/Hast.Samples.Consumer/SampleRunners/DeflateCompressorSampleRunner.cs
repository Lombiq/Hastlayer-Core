using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;
using Hast.Samples.SampleAssembly.Deflate;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal static class DeflateCompressorSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.Deflate.DeflateCompressor");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var deflateCompressor = await hastlayer.GenerateProxy(hardwareRepresentation, new DeflateCompressor());

            var result = deflateCompressor.Deflate(File.ReadAllBytes("Uncompressed.txt"));
        }
    }
}
