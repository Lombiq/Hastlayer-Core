using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Communication;
using Hast.Samples.Kpz;
using Hast.Samples.SampleAssembly;
using Lombiq.OrchardAppHost;
using NUnit.Framework;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class SampleAssemblyVerificationTests : VerificationTestFixtureBase
    {
        public SampleAssemblyVerificationTests()
        {
            var baseRegistrationBuilder = _shellRegistrationBuilder;
            _shellRegistrationBuilder = builder =>
            {
                baseRegistrationBuilder(builder);
                builder.RegisterType<MemberSuitabilityChecker>().As<IMemberSuitabilityChecker>();
            };
        }

        [Test]
        public async Task SampleAssembliesMatchApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(PrimeCalculator).Assembly, typeof(KpzKernelsInterface).Assembly },
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

                        configuration.AddHardwareEntryPointType<KpzKernelsInterface>();
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
