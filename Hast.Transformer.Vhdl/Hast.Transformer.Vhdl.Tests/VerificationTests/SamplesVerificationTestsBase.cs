using Hast.Algorithms;
using Hast.Algorithms.Random;
using Hast.Common.Models;
using Hast.Layer;
using Hast.Samples.FSharpSampleAssembly;
using Hast.Samples.Kpz.Algorithms;
using Hast.Samples.SampleAssembly;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Lombiq.Arithmetics;
using Lombiq.OrchardAppHost;
using System.Collections.Immutable;
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


        protected Task<VhdlHardwareDescription> CreateSourceForBasicSamples() =>
            _host.RunGet(wc => TransformAssembliesToVhdl(
                wc.Resolve<ITransformer>(),
                new[] { typeof(PrimeCalculator).Assembly, typeof(RandomMwc64X).Assembly },
                configuration =>
                {
                    // Only testing well-tested samples.

                    var transformerConfiguration = configuration.TransformerConfiguration();


                    configuration.AddHardwareEntryPointType<GenomeMatcher>();

                    // Not configuring MaxDegreeOfParallelism for ImageContrastModifier to also test the logic that
                    // can figure it out.
                    configuration.AddHardwareEntryPointType<ImageContrastModifier>();

                    configuration.AddHardwareEntryPointType<Loopback>();

                    configuration.AddHardwareEntryPointType<MemoryTest>();

                    configuration.AddHardwareEntryPointType<MonteCarloPiEstimator>();
                    transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                        new MemberInvocationInstanceCountConfigurationForMethod<MonteCarloPiEstimator>(m => m.EstimatePi(null), 0)
                        {
                            MaxDegreeOfParallelism = 3 // Using a smaller degree because we don't need excess repetition.
                        });
                    configuration.TransformerConfiguration().AddAdditionalInlinableMethod<RandomXorshiftLfsr16>(p => p.NextUInt16());

                    configuration.AddHardwareEntryPointType<ObjectOrientedShowcase>();

                    configuration.AddHardwareEntryPointType<ParallelAlgorithm>();
                    transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                        new MemberInvocationInstanceCountConfigurationForMethod<ParallelAlgorithm>(p => p.Run(null), 0)
                        {
                            MaxDegreeOfParallelism = 3
                        });

                    configuration.AddHardwareEntryPointType<PrimeCalculator>();
                    transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                        new MemberInvocationInstanceCountConfigurationForMethod<PrimeCalculator>(p => p.ParallelizedArePrimeNumbers(default(Transformer.Abstractions.SimpleMemory.SimpleMemory)), 0)
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
                }));

        protected async Task<string> CreateVhdlForKpzSamples()
        {
            var notInlinedSource = await _host.RunGet(async wc =>
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

            var inlinedSource = await _host.RunGet(async wc =>
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
                        configuration.TransformerConfiguration().AddAdditionalInlinableMethod<RandomMwc64X>(r => r.NextUInt32());
                    });

                return hardwareDescription.VhdlSource;
            });

            return notInlinedSource + inlinedSource;
        }

        protected Task<string> CreateVhdlForUnumSample() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Unum).Assembly, typeof(ImmutableArray).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<UnumCalculator>();

                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            UnumCalculator.EnvironmentFactory().EmptyBitMask.SegmentCount,
                            UnumCalculatorExtensions.ManuallySizedArrays);
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<string> CreateVhdlForPositSample() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly, typeof(ImmutableArray).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<PositCalculator>();
                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            PositCalculator.EnvironmentFactory().EmptyBitMask.SegmentCount,
                            PositCalculatorExtensions.ManuallySizedArrays);
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<string> CreateVhdlForPosit32Sample() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<Posit32Calculator>();
                        configuration.TransformerConfiguration().EnableMethodInlining = false;

                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<Posit32Calculator>(p => p.ParallelizedCalculateIntegerSumUpToNumbers(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<string> CreateVhdlForPosit32SampleWithInlining() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<Posit32Calculator>();

                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<Posit32Calculator>(p => p.ParallelizedCalculateIntegerSumUpToNumbers(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<VhdlHardwareDescription> CreateSourceForAdvancedPosit32Sample() =>
            _host.RunGet(wc => TransformAssembliesToVhdl(
                wc.Resolve<ITransformer>(),
                new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly },
                configuration =>
                {
                    configuration.AddHardwareEntryPointType<Posit32AdvancedCalculator>();
                    configuration.TransformerConfiguration().EnableMethodInlining = false;
                }));

        protected async Task<string> CreateVhdlForPosit32FusedSample()
        {
            var notInlinedSource = await _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<Posit32FusedCalculator>();
                        configuration.TransformerConfiguration().EnableMethodInlining = false;
                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            Posit32.QuireSize >> 6,
                            Posit32FusedCalculatorExtensions.ManuallySizedArrays);
                    });

                return hardwareDescription.VhdlSource;
            });

            var inlinedSource = await _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<Posit32FusedCalculator>();
                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            Posit32.QuireSize >> 6,
                            Posit32FusedCalculatorExtensions.ManuallySizedArrays);
                    });

                return hardwareDescription.VhdlSource;
            });

            return notInlinedSource + inlinedSource;
        }

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
                            new MemberInvocationInstanceCountConfigurationForMethod<Fix64Calculator>(f => f.ParallelizedCalculateIntegerSumUpToNumbers(default(Transformer.Abstractions.SimpleMemory.SimpleMemory)), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                return hardwareDescription.VhdlSource;
            });

        protected Task<string> CreateVhdlForFSharpSamples() =>
            _host.RunGet(async wc =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    wc.Resolve<ITransformer>(),
                    new[] { typeof(FSharpParallelAlgorithmContainer).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<FSharpParallelAlgorithmContainer.FSharpParallelAlgorithm>();

                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<FSharpParallelAlgorithmContainer.FSharpParallelAlgorithm>(f => f.Run(null), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                return hardwareDescription.VhdlSource;
            });
    }
}
