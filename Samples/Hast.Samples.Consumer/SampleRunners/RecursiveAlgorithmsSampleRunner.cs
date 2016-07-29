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
    internal static class RecursiveAlgorithmsSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.RecursiveAlgorithms");

            configuration.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Add(
                new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.RecursiveAlgorithms.Recursively")
                {
                    // If we give these algorithms inputs causing a larger recursion depth then that will
                    // cause runtime problems.
                    MaxRecursionDepth = 20
                });
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var recursiveAlgorithms = await hastlayer.GenerateProxy(hardwareRepresentation, new RecursiveAlgorithms());

            var fibonacci = recursiveAlgorithms.CalculateFibonacchiSeries(13); // 233
            var factorial = recursiveAlgorithms.CalculateFactorial(6); // 720 
        }
    }
}
