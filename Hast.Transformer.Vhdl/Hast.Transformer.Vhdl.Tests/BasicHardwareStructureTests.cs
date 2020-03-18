using Hast.Common.Models;
using Hast.Layer;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Hast.Transformer.Abstractions;
using Hast.VhdlBuilder.Testing;
using Xunit;
using Shouldly;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Tests
{
    public class BasicHardwareStructureTests : VhdlTransformingTestFixtureBase
    {
        [Fact]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            await Host.RunAsync<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformClassStrutureExamplesToVhdl(transformer);

                hardwareDescription.Language.ShouldBe("VHDL");
                hardwareDescription.HardwareEntryPointNamesToMemberIdMappings.Count.ShouldBe(14);
                hardwareDescription.VhdlSource.ShouldNotBeNullOrEmpty();
                //hardwareDescription.VhdlManifestIfFresh.ShouldNotBeNull(); // Since caching is off.
            });
        }

        [Fact]
        public async Task BasicVhdlStructureIsCorrect()
        {
            await Task.FromResult(true);
            //await Host.RunAsync<ITransformer>(async transformer =>
            //{
            //    var topModule = (Module)(await TransformClassStrutureExamplesToVhdl(transformer)).VhdlManifestIfFresh.Modules.Last();

            //    var architecture = topModule.Architecture;
            //    architecture.Name.ShouldNotBeNullOrEmpty();
            //    architecture.Declarations.ShouldRecursivelyContain(element => element is Signal);
            //    architecture.Body.ShouldRecursivelyContain<Process>(p => p.Name.Contains("ExternalInvocationProxy"));

            //    var entity = topModule.Entity;
            //    entity.Name.ShouldNotBeNullOrEmpty();
            //    entity.Ports.Count.ShouldBe(5);
            //    entity.ShouldBe(topModule.Architecture.Entity, "The top module's entity is not referenced by the architecture.");

            //    topModule.Libraries.Any().ShouldBeTrue();
            //});
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
