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
        public async Task UnumSampleMatchesApproved()
        {
            (await CreateVhdlForUnumSample()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task PositSampleMatchesApproved()
        {
            (await CreateVhdlForPositSample()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task Posit32SampleMatchesApproved()
        {
            (await CreateVhdlForPosit32Sample()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task Posit32SampleWithInliningMatchesApproved()
        {
            (await CreateVhdlForPosit32SampleWithInlining()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        [Ignore]
        public async Task Posit32FusedSampleMatchesApproved()
        {
            (await CreateVhdlForPosit32FusedSample()).ShouldMatchApprovedWithVhdlConfiguration();
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
