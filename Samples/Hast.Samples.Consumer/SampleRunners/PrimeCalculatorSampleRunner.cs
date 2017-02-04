using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.SampleAssembly;

namespace Hast.Samples.Consumer.SampleRunners
{
    internal static class PrimeCalculatorSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator.IsPrimeNumber");

            configuration.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Add(
                new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.PrimeCalculator.ParallelizedArePrimeNumbers.LambdaExpression.0")
                {
                    MaxDegreeOfParallelism = PrimeCalculator.MaxDegreeOfParallelism
                });
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new PrimeCalculator());

            var isPrime = primeCalculator.IsPrimeNumber(0);
            var isPrime2 = primeCalculator.IsPrimeNumber(4);
        }
    }
}
