using Hast.Catapult;
using Hast.Catapult.Abstractions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [Ignore("Not ready.")]
    [TestFixture]
    public class CatapultSamplesVerificationTests : SamplesVerificationTestsBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;
        protected override string DeviceName => CatapultManifestProvider.DeviceName;


        public CatapultSamplesVerificationTests()
        {
            _requiredExtension.AddRange(new[]
            {
                typeof(CatapultDriver).Assembly
            });
        }


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
