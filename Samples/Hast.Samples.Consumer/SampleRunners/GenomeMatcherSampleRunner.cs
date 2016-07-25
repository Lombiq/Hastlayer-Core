using System;
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
    internal static class GenomeMatcherSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.GenomeMatcher");
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var genomeMatcher = await hastlayer.GenerateProxy(hardwareRepresentation, new GenomeMatcher());

            // Sample from IBM.
            var inputOne = "GCCCTAGCG";
            var inputTwo = "GCGCAATG";

            var result = genomeMatcher.CalculateLongestCommonSubsequence(inputOne, inputTwo);

            // Sample from Wikipedia.
            inputOne = "ACACACTA";
            inputTwo = "AGCACACA";

            result = genomeMatcher.CalculateLongestCommonSubsequence(inputOne, inputTwo);

            inputOne = "lombiqtech";
            inputTwo = "coulombtech";

            result = genomeMatcher.CalculateLongestCommonSubsequence(inputOne, inputTwo);
        }
    }
}
