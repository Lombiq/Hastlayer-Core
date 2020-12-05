using Hast.Xilinx.Abstractions.ManifestProviders;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    public class XilinxSamplesVerificationTests : SamplesVerificationTestsBase
    {
        public static IEnumerable<object[]> AllDevices =>
            new[]
            {
                new object[] { Nexys4DdrManifestProvider.DeviceName },
                new object[] { AlveoU50ManifestProvider.DeviceName },
                new object[] { AlveoU200ManifestProvider.DeviceName },
                new object[] { AlveoU250ManifestProvider.DeviceName },
                new object[] { AlveoU280ManifestProvider.DeviceName },
                new object[] { AwsF1ManifestProvider.DeviceName },
            };

        [Theory, MemberData(nameof(AllDevices))]
        public async Task BasicSamplesMatchApproved(string deviceName) =>
            (await CreateSourceForBasicSamples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task KpzSamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForKpzSamples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task UnumSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForUnumSample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task PositSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPositSample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32SampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32Sample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32AdvancedSampleMatchesApproved(string deviceName) =>
            (await CreateSourceForAdvancedPosit32Sample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32SampleWithInliningMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32SampleWithInlining(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32FusedSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32FusedSample(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Fix64SamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForFix64Samples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task FSharpSamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForFSharpSamples(deviceName)).ShouldMatchApprovedWithVhdlConfiguration(deviceName);
    }
}
