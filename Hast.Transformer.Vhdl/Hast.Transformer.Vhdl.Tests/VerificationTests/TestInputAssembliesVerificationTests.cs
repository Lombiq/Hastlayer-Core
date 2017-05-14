using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Lombiq.OrchardAppHost;
using Hast.TestInputs.ClassStructure2;
using Hast.TestInputs.ClassStructure1;
using Shouldly;
using System.Text.RegularExpressions;
using Hast.Common.Configuration;

namespace Hast.Transformer.Vhdl.Tests.VerificationTests
{
    [TestFixture]
    public class TestInputAssembliesVerificationTests : VerificationTestFixtureBase
    {
        [Test]
        public async Task ClassStructureTestInputsMatchApproved()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformAssembliesToVhdl(
                    transformer,
                    new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly },
                    configuration =>
                    {
                        configuration.TransformerConfiguration().UseSimpleMemory = false;
                    });

                hardwareDescription.VhdlSource.ShouldMatchApprovedWithVhdlConfiguration();
            });
        }
    }
}
