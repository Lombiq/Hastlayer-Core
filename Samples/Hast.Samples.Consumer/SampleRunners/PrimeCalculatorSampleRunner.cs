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
            // connected then all of them will be utilized. If the whole device pool is utilized calls will
            // wait for their turn.
            // Uncomment if you have mulitple boards connected.
            //var parallelLaunchedIsPrimeTasks = new List<Task<bool>>();
            //for (uint i = 100; i < 110; i++)
            //{
            //    parallelLaunchedIsPrimeTasks
            //        .Add(Task.Factory.StartNew(indexObject => primeCalculator.IsPrimeNumber((uint)indexObject), i));
            //}
            //var parallelLaunchedArePrimes = await Task.WhenAll(parallelLaunchedIsPrimeTasks);


            // In-algorithm parallelization:
            // Note that if the amount of numbers used here can't be divided by PrimeCalculator.MaxDegreeOfParallelism 
            // then for ParallelizedArePrimeNumbers the input and output will be padded to a divisible amount (see 
            // comments in the method). Thus the communication roundtrip will be slower for ParallelizedArePrimeNumbers.
            // Because of this since PrimeCalculator.MaxDegreeOfParallelism is 35 we use 35 numbers here.
            var numbers = new uint[]
            {
                9749, 9973, 902119, 907469, 915851,
                9749, 9973, 902119, 907469, 915851,
                9749, 9973, 902119, 907469, 915851,
                9749, 9973, 902119, 907469, 915851,
                9749, 9973, 902119, 907469, 915851,
                9749, 9973, 902119, 907469, 915851,
                9749, 9973, 902119, 907469, 915851
            };

            var arePrimes3 = primeCalculator.ArePrimeNumbers(numbers);
            var arePrimes4 = primeCalculator.ParallelizedArePrimeNumbers(numbers);
        }
    }
}
