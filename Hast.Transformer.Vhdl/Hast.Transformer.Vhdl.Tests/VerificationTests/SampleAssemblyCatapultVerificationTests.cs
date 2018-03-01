using System.Threading.Tasks;
using Hast.Catapult;
using Hast.Layer;
using Hast.Samples.SampleAssembly;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Lombiq.OrchardAppHost;
using NUnit.Framework;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class SampleAssemblyCatapultVerificationTests : VerificationTestFixtureBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;


        public SampleAssemblyCatapultVerificationTests()
        {
            _requiredExtension.AddRange(new[]
            {
                typeof(CatapultDriver).Assembly
            });
        }


        [Test]
        public async Task BasicCatapultSamplesMatchApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(PrimeCalculator).Assembly },
                    configuration =>
                    {
                        configuration.DeviceName = "Catapult";

                        // Only testing well-tested samples.

                        var transformerConfiguration = configuration.TransformerConfiguration();

                        configuration.AddHardwareEntryPointType<PrimeCalculator>();
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<PrimeCalculator>(p => p.ParallelizedArePrimeNumbers(null), 0)
                            {
                                MaxDegreeOfParallelism = 3 // Using a smaller degree because we don't need excess repetition.
                            });
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
    }
