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
    internal static class PrimeCalculatorSampleRunner
    {
        public static void Configure(HardwareGenerationConfiguration configuration)
        {
            configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator");

            configuration.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Add(
                new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.PrimeCalculator.ParallelizedArePrimeNumbers.LambdaExpression.0")
                {
                    MaxDegreeOfParallelism = PrimeCalculator.MaxDegreeOfParallelism
                });
        }

        public static async Task Run(IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation)
        {
            var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new PrimeCalculator());

            var isPrime = primeCalculator.IsPrimeNumber(15);
            var isPrime2 = primeCalculator.IsPrimeNumber(13);
            var isPrime3 = await primeCalculator.IsPrimeNumberAsync(21);
            // Only 2341 is prime.
            var arePrimes = primeCalculator.ArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 });
            var arePrimes2 = primeCalculator.ArePrimeNumbers(new uint[] { 13, 493 });

            // You can also launch hardware-executed method calls in parallel. If there are multiple boards
            // attached then all of them will be utilized. If the whole device pool is utilized calls will
            // wait for their turn.
            var parallelLaunchedIsPrimeTasks = new List<Task<bool>>();
            for (uint i = 100; i < 110; i++)
            {
                parallelLaunchedIsPrimeTasks
                    .Add(Task.Factory.StartNew(indexObject => primeCalculator.IsPrimeNumber((uint)indexObject), i));
            }
            var parallelLaunchedArePrimes = await Task.WhenAll(parallelLaunchedIsPrimeTasks);

            // With 210 numbers this takes about 2,1s all together (with UART) on an FPGA and 166s on a 
            // 3,2GHz i7.
            // With 4000 numbers it takes 38s on an FPGA and 3550s (about an hour) on the same PC. 10000 
            // numbers take 84s on an FPGA.
            // These take the following amount of time via Ethernet respectively: 330ms (200 numbers), 1,5s 
            // (4000 numbers), 6,8s (10000 numbers).
            // About 90000000 numbers are the maximum before an OutOfMemoryException down the line. But that 
            // would take 93 hours to send via 9600 baud serial (and then above this to receive the results).
            var numberCount = 210;
            var numbers = new uint[numberCount];
            for (uint i = (uint)(uint.MaxValue - numberCount); i < uint.MaxValue; i++)
            {
                numbers[i - (uint.MaxValue - numberCount)] = (uint)i;
            }
            var arePrimes3 = primeCalculator.ArePrimeNumbers(numbers);

            // With 210 numbers and 50 workers this takes about ...s all together (with UART) on an FPGA and 
            // 95s on a 3,2GHz i7.
            var arePrimes4 = await primeCalculator.ParallelizedArePrimeNumbers(numbers);
        }
    }
}
