using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Layer;
using Hast.Samples.SampleAssembly;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Hast.Transformer.Services;
using Lombiq.OrchardAppHost;
using Lombiq.Unum;
using NUnit.Framework;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class SampleAssemblyVerificationTests : VerificationTestFixtureBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;


        [Test]
        public async Task BasicSamplesMatchApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(PrimeCalculator).Assembly },
                    configuration =>
                    {
                        // Only testing well-tested samples.

                        var transformerConfiguration = configuration.TransformerConfiguration();

                        configuration.AddHardwareEntryPointType<ObjectOrientedShowcase>();

                        configuration.AddHardwareEntryPointType<ParallelAlgorithm>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<ParallelAlgorithm>(p => p.Run(null), 0)
                            {
                                MaxDegreeOfParallelism = 5 // Using a smaller degree because we don't need excess repetition.
                            });

                        configuration.AddHardwareEntryPointType<PrimeCalculator>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<PrimeCalculator>(p => p.ParallelizedArePrimeNumbers(null), 0)
                            {
                                MaxDegreeOfParallelism = 5
                            });

                        configuration.AddHardwareEntryPointType<RecursiveAlgorithms>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<RecursiveAlgorithms>("Recursively")
                            {
                                MaxRecursionDepth = 5
                            });

                        configuration.AddHardwareEntryPointType<SimdCalculator>();
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }

        [Test]
        public async Task UnumSampleMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Unum).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<UnumCalculator>();
                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            UnumCalculator.EnvironmentFactory().EmptyBitMask.SegmentCount,
                            UnumCalculatorExtensions.ManuallySizedArrays);
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
