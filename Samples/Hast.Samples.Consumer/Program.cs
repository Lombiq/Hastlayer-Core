using System;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Samples.Consumer.SampleRunners;
using Hast.Samples.SampleAssembly;
using Hast.Transformer.Vhdl.Configuration;
using Hast.VhdlBuilder.Representation;

namespace Hast.Samples.Consumer
{
    // In this simple console application we generate hardware from some sample algorithms.

    // Configure the whole sample project here:
    internal static class Configuration
    {
        /// <summary>
        /// Specify a path here where the VHDL file describing the hardware to be generated will be saved. If the path
        /// is relative (like the default) then the file will be saved along this project's executable in the bin output
        /// directory. If an empty string or null is specified then no file will be generated.
        /// </summary>
        public static string VhdlOutputFilePath = @"Hast_IP.vhd";

        /// <summary>
        /// Which sample algorithm to transform and run? Choose one.
        /// </summary>
        public static Sample SampleToRun = Sample.PrimeCalculator;
    }


    class Program
    {
        static void Main(string[] args)
        {
            // Wrapping the whole program into Task.Run() is a workaround for async just to be able to run all this from 
            // inside a console app.
            Task.Run(async () =>
                {
                    /*
                     * On a high level these are the steps to use Hastlayer:
                     * 1. Create the Hastlayer shell.
                     * 2. Configure hardware generation and generate FPGA hardware representation of the given .NET code.
                     * 3. Generate proxies for hardware-transformed types and use these proxies to utilize hardware
                     *    implementations. (You can see this inside the SampleRunners.)
                     */

                    // Inititializing a Hastlayer shell for Xilinx FPGA boards.
                    using (var hastlayer = Xilinx.HastlayerFactory.Create())
                    {
                        // Hooking into an event of Hastlayer so some execution information can be made visible on the
                        // console.
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


                        // Letting the configuration of samples run.
                        switch (Configuration.SampleToRun)
                        {
                            case Sample.GenomeMatcher:
                                GenomeMatcherSampleRunner.Configure(configuration);
                                break;
                            case Sample.HastlayerOptimizedAlgorithm:
                                HastlayerOptimizedAlgorithmSampleRunner.Configure(configuration);
                                break;
                            case Sample.ImageProcessingAlgorithms:
                                ImageProcessingAlgorithmsSampleRunner.Configure(configuration);
                                break;
                            case Sample.MonteCarloAlgorithm:
                                MonteCarloAlgorithmSampleRunner.Configure(configuration);
                                break;
                            case Sample.ObjectOrientedShowcase:
                                ObjectOrientedShowcaseSampleRunner.Configure(configuration);
                                break;
                            case Sample.PrimeCalculator:
                                PrimeCalculatorSampleRunner.Configure(configuration);
                                break;
                            case Sample.RecursiveAlgorithms:
                                RecursiveAlgorithmsSampleRunner.Configure(configuration);
                                break;
                            case Sample.SimdCalculator:
                                SimdCalculatorSampleRunner.Configure(configuration);
                                break;
                            default:
                                break;
                        }

                        // The generated VHDL code will contain debug-level information, though it will be a bit slower
                        // to create.
                        configuration.VhdlTransformerConfiguration().VhdlGenerationOptions = VhdlGenerationOptions.Debug;

                        #region Hack
                        // You just had to open this region, didn't you?
                        // If VHDL is written to a file just after GenerateHardware() somehow an exception will be 
                        // thrown from inside Hastlayer. This shouldn't happen and the exception has nothing to do with
                        // files. It's a mystery. But having this dummy file write here solves it.
                        //System.IO.File.WriteAllText(Configuration.VhdlOutputFilePath, "dummy");
                        #endregion

                        // Generating hardware from the sample assembly with the given configuration.
                        var hardwareRepresentation = await hastlayer.GenerateHardware(
                            new[]
                            {
                                // Selecting any type from the sample assembly here just to get its Assembly object.
                                typeof(PrimeCalculator).Assembly
                            },
                            configuration);


                        if (!string.IsNullOrEmpty(Configuration.VhdlOutputFilePath))
                        {
                            Helpers.HardwareRepresentationHelper.WriteVhdlToFile(hardwareRepresentation);
                        }


                        // Running samples.
                        switch (Configuration.SampleToRun)
                        {
                            case Sample.GenomeMatcher:
                                await GenomeMatcherSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.HastlayerOptimizedAlgorithm:
                                await HastlayerOptimizedAlgorithmSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.ImageProcessingAlgorithms:
                                await ImageProcessingAlgorithmsSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.MonteCarloAlgorithm:
                                await MonteCarloAlgorithmSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.ObjectOrientedShowcase:
                                await ObjectOrientedShowcaseSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.PrimeCalculator:
                                await PrimeCalculatorSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.RecursiveAlgorithms:
                                await RecursiveAlgorithmsSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            case Sample.SimdCalculator:
                                await SimdCalculatorSampleRunner.Run(hastlayer, hardwareRepresentation);
                                break;
                            default:
                                break;
                        }
                    }
                }).Wait();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
