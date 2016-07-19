﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal static class MonteCarloAlgorithmSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.MonteCarloAlgorithm");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var monteCarloAlgorithm = await hastlayer
                .GenerateProxy(hardwareRepresentation, new MonteCarloAlgorithm());
            var monteCarloResult = monteCarloAlgorithm.CalculateTorusSectionValues(5000000);
        }
    }
}