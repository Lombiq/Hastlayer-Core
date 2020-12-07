using Hast.TestInputs.Dynamic;
using Hast.Transformer.Abstractions;
using Xunit;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public class DynamicTestInputAssembliesVerificationTests : VerificationTestFixtureBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;

        [Fact]
        public async Task DynamicTestInputAssemblyMatchesApproved() => await Host.RunAsync<ITransformer>(async transformer =>
                                                                     {
                                                                         var hardwareDescription = await TransformAssembliesToVhdlAsync(
                                                                             transformer,
                                                                             new[] { typeof(BinaryAndUnaryOperatorExpressionCases).Assembly },
                                                                             configuration =>
                                                                             {
                                                                             });

                                                                         hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
                                                                     });
    }
}
