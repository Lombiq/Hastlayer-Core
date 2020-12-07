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
            (await CreateSourceForBasicSamplesAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task KpzSamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForKpzSamplesAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task UnumSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForUnumSampleAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task PositSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPositSampleAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32SampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32SampleAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32AdvancedSampleMatchesApproved(string deviceName) =>
            (await CreateSourceForAdvancedPosit32SampleAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32SampleWithInliningMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32SampleWithInliningAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Posit32FusedSampleMatchesApproved(string deviceName) =>
            (await CreateVhdlForPosit32FusedSampleAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task Fix64SamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForFix64SamplesAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);

        [Theory, MemberData(nameof(AllDevices))]
        [InlineData(Nexys4DdrManifestProvider.DeviceName)]
        public async Task FSharpSamplesMatchesApproved(string deviceName) =>
            (await CreateVhdlForFSharpSamplesAsync()).ShouldMatchApprovedWithVhdlConfiguration(deviceName);
    }
}
