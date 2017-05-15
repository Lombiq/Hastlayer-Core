using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Communication;
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
                    new[] { typeof(PrimeCalculator).Assembly },
                    configuration =>
                    {
                        // Only testing well-tested samples.

                        var transformerConfiguration = configuration.TransformerConfiguration();

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.ObjectOrientedShowcase");

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm");
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm.Run.LambdaExpression.0")
                            {
                                MaxDegreeOfParallelism = 5 // Using a smaller degree because we don't need excess repetition.
                            });

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.PrimeCalculator");
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.PrimeCalculator.ParallelizedArePrimeNumbers.LambdaExpression.0")
                            {
                                MaxDegreeOfParallelism = 5
                            });

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.RecursiveAlgorithms");
                        transformerConfiguration.AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfiguration("Hast.Samples.SampleAssembly.RecursiveAlgorithms.Recursively")
                            {
                                MaxRecursionDepth = 5
                            });

                        configuration.PublicHardwareMemberNamePrefixes.Add("Hast.Samples.SampleAssembly.SimdCalculator");
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
