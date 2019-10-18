﻿using Hast.Catapult;
using NUnit.Framework;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class CatapultSamplesVerificationTests : SamplesVerificationTestsBase
    {
        protected override bool UseStubMemberSuitabilityChecker => false;
        protected override string DeviceName => "Catapult";


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
        public async Task KpzSampleMatchesApproved()
        {
            (await CreateVhdlForKpzSamples()).ShouldMatchApprovedWithVhdlConfiguration();
        }

        [Test]
        public async Task Fix64SampleMatchesApproved()
        {
            (await CreateVhdlForFix64Samples()).ShouldMatchApprovedWithVhdlConfiguration();
        }
    }
}