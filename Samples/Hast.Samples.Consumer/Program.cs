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
using Hast.Tests.TestAssembly1.ComplexTypes;
using Hast.Tests.TestAssembly2;
using System.Drawing;
using Hast.VhdlBuilder.Representation;

namespace Hast.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
                {
                    // Generating hardware from samples:
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        hastlayer.ExecutedOnHardware += (sender, e) =>
                            {
                                Console.WriteLine(
                                    "Executing " +
                                    e.MemberFullName +
                                    " on hardware took " +
                                    e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds +
                                    "ms (net) " +
                                    e.HardwareExecutionInformation.FullExecutionTimeMilliseconds +
                                    " milliseconds (all together)");
                            };


                        var configuration = new HardwareGenerationConfiguration();

                        //configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.MonteCarloAlgorithm");
                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator");
                        //configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.RecursiveAlgorithms");

                        configuration.TransformerConfiguration().MemberInvokationInstanceCountConfigurations.Add(
                            new MemberInvokationInstanceCountConfiguration("Hast.Samples.SampleAssembly.PrimeCalculator.IsPrimeNumberInternal")
                            {
                                MaxDegreeOfParallelism = PrimeCalculator.MaxDegreeOfParallelism
                            });
                        configuration.TransformerConfiguration().MemberInvokationInstanceCountConfigurations.Add(
                            new MemberInvokationInstanceCountConfiguration("Hast.Samples.SampleAssembly.RecursiveAlgorithms")
                            {
                                // If we give these algorithms inputs causing a larger recursion depth then that will
                                // cause runtime problems.
                                MaxRecursionDepth = 20
                            });


                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly
                            },
                            configuration);


                        File.WriteAllText(@"E:\Projects\Munka\Lombiq\Hastlayer\HastlayerHardwareTest2\Hastlayer.ip\Hast_IP.vhd", ToVhdl(hardwareRepresentation.HardwareDescription));
                        //File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", ToVhdl(hardwareRepresentation.HardwareDescription));


                        // For testing transformation, we don't need anything else.
                        return;

                        #region PrimeCalculator
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
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        //var arePrimes3 = primeCalculator.ArePrimeNumbers(numbers);
                        sw.Stop();

                        // With 210 numbers and 50 workers this takes about ...s all together (with UART) on an FPGA and 
                        // 95s on a 3,2GHz i7.
                        sw.Restart();
                        var arePrimes4 = await primeCalculator.ParallelizedArePrimeNumbers(numbers);
                        sw.Stop();
                        #endregion


                        #region RecursiveAlgorithms
                        var recursiveAlgorithms = await hastlayer.GenerateProxy(hardwareRepresentation, new RecursiveAlgorithms());

                        var fibonacci = recursiveAlgorithms.CalculateFibonacchiSeries((short)13); // 233
                        var factorial = recursiveAlgorithms.CalculateFactorial((short)6); // 720 
                        #endregion


                        #region ImageAlgorithms
                        using (var bitmap = new Bitmap("fpga.jpg"))
                        {
                            var imageContrastModifier = await hastlayer
                                .GenerateProxy(hardwareRepresentation, new ImageContrastModifier());
                            var modifiedImage = imageContrastModifier.ChangeImageContrast(bitmap, -50);

                            var imageFilter = await hastlayer.GenerateProxy(hardwareRepresentation, new ImageFilter());
                            var filteredImage = imageFilter.DetectHorizontalEdges(bitmap);
                        }
                        #endregion


                        #region GenomeMatcher
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
                        #endregion


                        #region MonteCarlo
                        var monteCarloAlgorithm = await hastlayer
                            .GenerateProxy(hardwareRepresentation, new MonteCarloAlgorithm());
                        var monteCarloResult = monteCarloAlgorithm.CalculateTorusSectionValues(5000000);
                        #endregion
                    }


                    // Generating hardware from test assemblies:
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        var configuration = new HardwareGenerationConfiguration
                        {
                            // Another way would be to add such prefixes (potentially for whole namespaces like here), 
                            // instead we add a single method below.
                            //PublicHardwareMemberPrefixes = new[] { "Hast.Tests.TestAssembly1.ComplexTypes.ComplexTypeHierarchy" }
                        };
                        configuration.AddPublicHardwareMethod<IInterface1>(complex => complex.Interface1Method1());
                        configuration.TransformerConfiguration().UseSimpleMemory = false;

                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(ComplexTypeHierarchy).Assembly,
                                typeof(StaticReference).Assembly
                            }, configuration);


                        // With this interface-typed variable we simulate that the object comes from dependency injection.
                        IInterface1 complexType = new ComplexTypeHierarchy();
                        complexType = await hastlayer.GenerateProxy(hardwareRepresentation, complexType);
                        var output = complexType.Interface1Method1();
                        output = complexType.Interface1Method2();
                    }

                }).Wait(); // This is a workaround for async just to be able to run all this from inside a console app.
        }


        private static string ToVhdl(IHardwareDescription hardwareDescription)
        {
            return ((Hast.Transformer.Vhdl.Models.VhdlHardwareDescription)hardwareDescription)
                .Manifest.TopModule.ToVhdl(new VhdlGenerationOptions { FormatCode = true, NameShortener = VhdlGenerationOptions.SimpleNameShortener });
        }
    }
}
