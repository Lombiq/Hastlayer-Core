using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Lombiq.OrchardAppHost;
using NUnit.Framework;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class TestInputAssembliesVerificationTests : VerificationTestFixtureBase
    {
        [Test]
        public async Task ClassStructureTestInputsMatchApproved()
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
    }
}
