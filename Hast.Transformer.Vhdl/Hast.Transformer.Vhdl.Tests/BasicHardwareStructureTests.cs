using System.Linq;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Testing;
using Lombiq.OrchardAppHost;
using NUnit.Framework;
using Shouldly;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class BasicHardwareStructureTests : VhdlTransformingTestFixtureBase
    {
        [Test]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformClassStrutureExamplesToVhdl(transformer);

                hardwareDescription.Language.ShouldBe("VHDL");
                hardwareDescription.HardwareEntryPointNamesToMemberIdMappings.Count.ShouldBe(14);
                hardwareDescription.VhdlSource.ShouldNotBeNullOrEmpty();
                hardwareDescription.VhdlManifestIfFresh.ShouldNotBeNull(); // Since caching is off.
            });
        }

        [Test]
        public async Task BasicVhdlStructureIsCorrect()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var topModule = (await TransformClassStrutureExamplesToVhdl(transformer)).VhdlManifestIfFresh.TopModule;

                var architecture = topModule.Architecture;
                architecture.Name.ShouldNotBeNullOrEmpty();
                architecture.Declarations.ShouldRecursivelyContain(element => element is Signal);
                architecture.Body.ShouldRecursivelyContain<Process>(p => p.Name.Contains("ExternalInvocationProxy"));

                var entity = topModule.Entity;
                entity.Name.ShouldNotBeNullOrEmpty();
                entity.Ports.Count.ShouldBe(5);
                entity.ShouldBe(topModule.Architecture.Entity, "The top module's entity is not referenced by the architecture.");

                topModule.Libraries.Any().ShouldBeTrue();
            });
        }


        private Task<VhdlHardwareDescription> TransformClassStrutureExamplesToVhdl(ITransformer transformer)
        {
            return TransformAssembliesToVhdl(
                transformer,
                new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly },
                configuration =>
                {
                    configuration.TransformerConfiguration().UseSimpleMemory = false;
                });
        }
    }
}
