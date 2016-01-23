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
                                File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", ToVhdl(e.MaterializedHardware.HardwareRepresentation.HardwareDescription));
                                
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
                        configuration.PublicHardwareMemberPrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator.IsPrimeNumber");
                        configuration.TransformerConfiguration().MaxCallStackDepth = 3;
                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                typeof(PrimeCalculator).Assembly
                                //typeof(Math).Assembly // Would be needed for Math.Sqrt() but transforming that is not yet supported.
                            },
                            configuration);

                        // For testing transformation, we don't need anything else.
                        return;

                        var primeCalculator = await hastlayer.GenerateProxy(hardwareRepresentation, new PrimeCalculator());

                        var isPrime = primeCalculator.IsPrimeNumber(15);
                        var arePrimes = primeCalculator.ArePrimeNumbers(new uint[] { 15, 493, 2341, 99237 }); // Only 2341 is prime

                        using (var bitmap = new Bitmap("fpga.jpg"))
                        {
                            var imageContrastModifier = await hastlayer.GenerateProxy(hardwareRepresentation, new ImageContrastModifier());
                            var modifiedImage = imageContrastModifier.ChangeImageContrast(bitmap, -50);

                            var imageFilter = await hastlayer.GenerateProxy(hardwareRepresentation, new ImageFilter());
                            var filteredImage = imageFilter.DetectHorizontalEdges(bitmap);
                        }

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

                        var monteCarloAlgorithm = await hastlayer.GenerateProxy(hardwareRepresentation, new MonteCarloAlgorithm()); 
                        var monteCarloResult = monteCarloAlgorithm.CalculateTorusSectionValues(5000000);
                    }

                    // Generating hardware from test assemblies:
                    using (var hastlayer = Hast.Xilinx.HastlayerFactory.Create())
                    {
                        var configuration = new HardwareGenerationConfiguration
                        {
                            // Another way would be to add such prefixes (potentially for whole namespaces like here), instead we add a single
                            // method below.
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

                        //File.WriteAllText(@"D:\Users\Zoltán\Projects\Munka\Lombiq\Hastlayer\sigasi\Workspace\HastTest\Test.vhd", ToVhdl(hardwareRepresentation.HardwareDescription));

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
