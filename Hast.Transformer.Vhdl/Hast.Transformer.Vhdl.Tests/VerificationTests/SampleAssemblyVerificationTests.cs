using System.Threading.Tasks;
using Hast.Algorithms;
using Hast.Layer;
using Hast.Samples.Kpz;
using Hast.Samples.SampleAssembly;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Lombiq.OrchardAppHost;
using Lombiq.Arithmetics;
using NUnit.Framework;
using System.Collections.Immutable;

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

                        // Not configuring MaxDegreeOfParallelism for ImageContrastModifier to also test the logic that
                        // can figure it out.
                        configuration.AddHardwareEntryPointType<ImageContrastModifier>();

                        configuration.AddHardwareEntryPointType<Loopback>();

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

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
        
        [Test]
        public async Task KpzSampleMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(KpzKernelsInterface).Assembly, typeof(PrngMWC64X).Assembly },
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
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Unum).Assembly, typeof(ImmutableArray).Assembly },
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

        [Test]
        public async Task PositSampleMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly, typeof(ImmutableArray).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<PositCalculator>();
                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            PositCalculator.EnvironmentFactory().EmptyBitMask.SegmentCount,
                            PositCalculatorExtensions.ManuallySizedArrays);
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }

        [Test]
        public async Task Posit32SampleMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
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

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }

        [Test]
        public async Task Posit32SampleWithInliningMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
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

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }

        [Test]
        public async Task Posit32FusedSampleMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(PrimeCalculator).Assembly, typeof(Posit).Assembly },
                    configuration =>
                    {
                        configuration.AddHardwareEntryPointType<Posit32FusedCalculator>();
                        configuration.TransformerConfiguration().EnableMethodInlining = false;
                        configuration.TransformerConfiguration().AddLengthForMultipleArrays(
                            Posit32.QuireSize >> 6,
                            Posit32FusedCalculatorExtensions.ManuallySizedArrays);
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }

        [Test]
        public async Task Fix64SampleMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
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

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
