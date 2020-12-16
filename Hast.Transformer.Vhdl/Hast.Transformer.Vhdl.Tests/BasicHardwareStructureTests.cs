using Hast.Common.Models;
using Hast.Layer;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Testing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hast.Transformer.Vhdl.Tests
{
    public class BasicHardwareStructureTests : VhdlTransformingTestFixtureBase
    {
        [Fact]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            VhdlManifest manifest = null;

            _hostConfiguration.OnServiceRegistration += (configuration, services) =>
                services.AddSingleton(new EventHandler<ITransformedVhdlManifest>((sender, e) => manifest = e.Manifest));

            await Host.RunAsync<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformClassStrutureExamplesToVhdl(transformer);

                hardwareDescription.Language.ShouldBe("VHDL");
                hardwareDescription.HardwareEntryPointNamesToMemberIdMappings.Count.ShouldBe(14);
                hardwareDescription.VhdlSource.ShouldNotBeNullOrEmpty();
                manifest.ShouldNotBeNull(); // Since caching is off.
            });
        }

        [Fact]
        public async Task BasicVhdlStructureIsCorrect()
        {
            VhdlManifest manifest = null;

            _hostConfiguration.OnServiceRegistration += (configuration, services) =>
                services.AddSingleton(new EventHandler<ITransformedVhdlManifest>((sender, e) => manifest = e.Manifest));

            await Host.RunAsync<ITransformer>(async transformer =>
            {
                await TransformClassStrutureExamplesToVhdl(transformer);
                var topModule = (Module)manifest.Modules.Last();

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


        private Task<VhdlHardwareDescription> TransformClassStrutureExamplesToVhdl(ITransformer transformer) =>
            TransformAssembliesToVhdl(
                transformer,
                new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly },
                configuration => configuration.TransformerConfiguration().UseSimpleMemory = false);
    }
}
