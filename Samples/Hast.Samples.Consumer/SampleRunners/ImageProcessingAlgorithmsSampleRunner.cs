using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal static class ImageProcessingAlgorithmsSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.ImageContrastModifier");
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.ImageFilter");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            using (var bitmap = new Bitmap("fpga.jpg"))
            {
                var imageContrastModifier = await hastlayer
                    .GenerateProxy(hardwareRepresentation, new ImageContrastModifier());
                var modifiedImage = imageContrastModifier.ChangeImageContrast(bitmap, -50);

                var imageFilter = await hastlayer.GenerateProxy(hardwareRepresentation, new ImageFilter());
                var filteredImage = imageFilter.DetectHorizontalEdges(bitmap);
            }
        }
    }
}
