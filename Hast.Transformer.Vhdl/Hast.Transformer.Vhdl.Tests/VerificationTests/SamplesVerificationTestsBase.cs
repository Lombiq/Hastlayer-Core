using Hast.Algorithms;
using Hast.Algorithms.Random;
using Hast.Layer;
using Hast.Samples.Kpz;
using Hast.Samples.Kpz.Algorithms;
using Hast.Samples.SampleAssembly;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Lombiq.OrchardAppHost;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    /// <summary>
    /// Base for tests that cover the samples. Needs to be done in such a way, test methods can't be in the base class
    /// (NUnit limitation), nor can Shouldly matching happen here (since it needs to be configured from the actual
    /// caller).
    /// </summary>
    public abstract class SamplesVerificationTestsBase : VerificationTestFixtureBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;


        protected Task<string> CreateVhdlForBasicSamples() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(RandomMwc64X).Assembly },
                    configuration =>
                    {
                        // Only testing well-tested samples.

                        var transformerConfiguration = configuration.TransformerConfiguration();

                        // Not configuring MaxDegreeOfParallelism for ImageContrastModifier to also test the logic that
                        // can figure it out.
                        configuration.AddHardwareEntryPointType<ImageContrastModifier>();

                        configuration.AddHardwareEntryPointType<Loopback>();

                        configuration.AddHardwareEntryPointType<MonteCarloPiEstimator>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<MonteCarloPiEstimator>(m => m.EstimatePi(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                        configuration.TransformerConfiguration().AddAdditionalInlinableMethod<RandomXorshiftLfsr16>(p => p.NextUInt16());

                        configuration.AddHardwareEntryPointType<ObjectOrientedShowcase>();

                        configuration.AddHardwareEntryPointType<ParallelAlgorithm>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<ParallelAlgorithm>(p => p.Run(null), 0)
                            {
                                MaxDegreeOfParallelism = 3 // Using a smaller degree because we don't need excess repetition.
                            });

                        configuration.AddHardwareEntryPointType<PrimeCalculator>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<PrimeCalculator>(p => p.ParallelizedArePrimeNumbers(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });

                        configuration.AddHardwareEntryPointType<RecursiveAlgorithms>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<RecursiveAlgorithms>("Recursively")
                            {
                                MaxRecursionDepth = 3
                            });

                        configuration.AddHardwareEntryPointType<SimdCalculator>();
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<string> CreateVhdlForKpzSamples() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(KpzKernelsInterface).Assembly, typeof(RandomMwc64X).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<KpzKernelsInterface>();

                        configuration.AddHardwareEntryPointType<KpzKernelsParallelizedInterface>();
                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<KpzKernelsParallelizedInterface>(p => p.ScheduleIterations(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<string> CreateVhdlForFix64Samples() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Fix64).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<Fix64Calculator>();

                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<Fix64Calculator>(f => f.ParallelizedCalculateIntegerSumUpToNumbers(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                return hardwareDescription.VhdlSource;
            });
    }
}
