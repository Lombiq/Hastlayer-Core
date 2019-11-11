using NUnit.Framework;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class XilinxSamplesVerificationTests : SamplesVerificationTestsBase
    {
        [Test]
        public async Task BasicSamplesMatchApproved()
        {
            (await CreateVhdlForBasicSamples()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task KpzSamplesMatchesApproved()
        {
            (await CreateVhdlForKpzSamples()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task Fix64SamplesMatchesApproved()
        {
            (await CreateVhdlForFix64Samples()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task FSharpSamplesMatchesApproved()
        {
            (await CreateVhdlForFSharpSamples()).ShouldMatchApprovedWithVhdlConfiguration();
        }
    }
}
