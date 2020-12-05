using Hast.Layer;
using Hast.Transformer.Vhdl.Abstractions.Configuration;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hast.TestInputs.Dynamic;

namespace Hast.DynamicTests
{
    internal static class TestExecutor
    {
        public static Task ExecuteSelectedTest<T>(Expression<Action<T>> caseSelector, Action<T> testExecutor)
            where T : DynamicTestInputBase, new() =>
            ExecuteTest(configuration => configuration.AddHardwareEntryPointMethod(caseSelector), testExecutor);

        public static async Task ExecuteTest<T>(Action<HardwareGenerationConfiguration> configurator, Action<T> testExecutor)
            where T : DynamicTestInputBase, new()
        {
            using var hastlayer = Hastlayer.Create();
            var configuration = new HardwareGenerationConfiguration("Nexys A7", "HardwareFramework");

            configurator(configuration);

            configuration.VhdlTransformerConfiguration().VhdlGenerationConfiguration = VhdlGenerationConfiguration.Debug;

            // var folderName = configuration.HardwareEntryPointMemberFullNames.Single();
            // var methodNameStartIndex = folderName.IndexOf("::");
            // folderName = folderName.Substring(methodNameStartIndex + 2, folderName.IndexOf("(") - 2 - methodNameStartIndex);
            // configuration.HardwareFrameworkPath = $@"E:\ShortPath\BinaryAndUnaryTests\{folderName}";

            hastlayer.ExecutedOnHardware += (sender, e) =>
            {
                Console.WriteLine(
                    "Executing on hardware took " +
                    e.HardwareExecutionInformation.HardwareExecutionTimeMilliseconds +
                    " milliseconds (net) " +
                    e.HardwareExecutionInformation.FullExecutionTimeMilliseconds +
                    " milliseconds (all together).");
            };

            Console.WriteLine("Hardware generation starts.");
            var hardwareRepresentation = await hastlayer.GenerateHardware(
                new[]
                {
                    typeof(T).Assembly,
                },
                configuration);

            Console.WriteLine("Hardware generated, starting hardware execution.");
            var proxyGenerationConfiguration = new ProxyGenerationConfiguration { VerifyHardwareResults = true };
            var hardwareInstance = await hastlayer.GenerateProxy(
                hardwareRepresentation,
                new T(),
                proxyGenerationConfiguration);

            hardwareInstance.Hastlayer = hastlayer;
            hardwareInstance.HardwareGenerationConfiguration = configuration;
            testExecutor(hardwareInstance);

            Console.WriteLine("Hardware execution finished.");
            Console.ReadKey();
        }
    }
}
