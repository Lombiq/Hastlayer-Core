using System.Threading.Tasks;
using Xunit;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public class XilinxSamplesVerificationTests : SamplesVerificationTestsBase
    {
        [Fact]
        public async Task BasicSamplesMatchApproved() =>
            (await CreateSourceForBasicSamples()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task KpzSamplesMatchesApproved() =>
            (await CreateVhdlForKpzSamples()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task UnumSampleMatchesApproved() =>
            (await CreateVhdlForUnumSample()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task PositSampleMatchesApproved() =>
            (await CreateVhdlForPositSample()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task Posit32SampleMatchesApproved() =>
            (await CreateVhdlForPosit32Sample()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task Posit32AdvancedSampleMatchesApproved() =>
            (await CreateSourceForAdvancedPosit32Sample()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task Posit32SampleWithInliningMatchesApproved() =>
            (await CreateVhdlForPosit32SampleWithInlining()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task Posit32FusedSampleMatchesApproved() =>
            (await CreateVhdlForPosit32FusedSample()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task Fix64SamplesMatchesApproved() =>
            (await CreateVhdlForFix64Samples()).ShouldMatchApprovedWithVhdlConfiguration();

        [Fact]
        public async Task FSharpSamplesMatchesApproved() =>
            (await CreateVhdlForFSharpSamples()).ShouldMatchApprovedWithVhdlConfiguration();
    }
}
