using Hast.Xilinx.Abstractions.ManifestProviders;
using System.Threading.Tasks;
using Xunit;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public class XilinxSamplesVerificationTests : SamplesVerificationTestsBase
    {
        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task BasicSamplesMatchApproved(string deviceName) =>
            (await CreateSourceForBasicSamples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task KpzSamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForKpzSamples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task UnumSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForUnumSample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task PositSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPositSample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32SampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32Sample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32AdvancedSampleMatchesApproved(string deviceName) =>
            (await CreateSourceForAdvancedPosit32Sample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32SampleWithInliningMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32SampleWithInlining(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32FusedSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32FusedSample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Fix64SamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForFix64Samples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task FSharpSamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForFSharpSamples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);
    }
}
