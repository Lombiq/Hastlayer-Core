using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure2;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Events;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Testing;
using ICSharpCode.NRefactory.CSharp;
using Lombiq.OrchardAppHost;
using NUnit.Framework;
using Shouldly;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class TransformedVhdlManifestBuilderBasicTests : TransformedVhdlManifestBuilderTestsBase
    {
        public TransformedVhdlManifestBuilderBasicTests() : base()
        {
        }


        [Test]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformReferenceAssembliesToVhdl(transformer);

                hardwareDescription.Language.ShouldBe("VHDL");
                hardwareDescription.HardwareMembers.Count().ShouldBe(18);
                hardwareDescription.MemberIdTable.Values.Count().ShouldBe(18);
                hardwareDescription.VhdlSource.ShouldNotBeNullOrEmpty();
                hardwareDescription.VhdlManifestIfFresh.ShouldNotBeNull(); // Since caching is off.
            });
        }

        [Test]
        public async Task BasicVhdlStructureIsCorrect()
        {
            await _host.Run<ITransformer, IVhdlTransformationEventHandler>(async (transformer, eventHandler) =>
            {
                var topModule = (await TransformReferenceAssembliesToVhdl(transformer)).VhdlManifestIfFresh.TopModule;

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


        private async Task<VhdlHardwareDescription> TransformReferenceAssembliesToVhdl(ITransformer transformer)
        {
            var configuration = new HardwareGenerationConfiguration { EnableCaching = false };
            configuration.TransformerConfiguration().UseSimpleMemory = false;
            return (VhdlHardwareDescription)await transformer.Transform(new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly }, configuration);
        }
    }
}
