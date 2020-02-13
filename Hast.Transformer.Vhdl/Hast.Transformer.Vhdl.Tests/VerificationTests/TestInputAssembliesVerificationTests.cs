using System.Threading.Tasks;
using Hast.Layer;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Hast.TestInputs.Various;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using NUnit.Framework;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class TestInputAssembliesVerificationTests : VerificationTestFixtureBase
    {
        [Test]
        public async Task TestInputAssembliesMatchApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly, typeof(CastingCases).Assembly },
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
