using Hast.Layer;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Hast.TestInputs.Static;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Lombiq.OrchardAppHost;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class StaticTestInputAssembliesVerificationTests : VerificationTestFixtureBase
    {
        [Test]
        public async Task ClassStructureAssembliesMatchApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly },
                    configuration =>
                    {
                        configuration.TransformerConfiguration().UseSimpleMemory = false;
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }

        [Test]
        public async Task StaticTestInputAssemblyMatchesApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(ArrayUsingCases).Assembly },
                    configuration =>
                    {
                        configuration.TransformerConfiguration().UseSimpleMemory = false;

                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<ParallelCases>(p => p.WhenAllWhenAnyAwaitedTasks(0), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                        configuration.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                            new MemberInvocationInstanceCountConfigurationForMethod<ParallelCases>(p => p.ObjectUsingTasks(0), 0)
                            {
                                MaxDegreeOfParallelism = 3
                            });
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
