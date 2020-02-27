using Hast.TestInputs.Dynamic;
using Hast.Transformer.Abstractions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class DynamicTestInputAssembliesVerificationTests : VerificationTestFixtureBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;

        [Test]
        public async Task DynamicTestInputAssemblyMatchesApproved()
        {
            await _host.RunAsync<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(BinaryAndUnaryOperatorExpressionCases).Assembly },
                    configuration =>
                    {
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
