﻿using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal static class ObjectOrientedShowcaseSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.AddPublicHardwareType<ObjectOrientedShowcase>();
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var ooShowcase = await hastlayer
                .GenerateProxy(hardwareRepresentation, new ObjectOrientedShowcase());
            var sum = ooShowcase.Run(93); // 268
        }
    }
}
