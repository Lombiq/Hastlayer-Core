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
    public class TransformedVhdlManifestBuilderTests : IntegrationTestBase
    {
        public TransformedVhdlManifestBuilderTests()
        {
            _requiredExtension.AddRange(new[] { typeof(ITransformer).Assembly, typeof(MemberIdTable).Assembly });

            _shellRegistrationBuilder = builder =>
            {
                builder.RegisterInstance(new StubMemberSuitabilityChecker()).As<IMemberSuitabilityChecker>();
            };
        }


        [Test]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformReferenceAssembliesToVhdl(transformer);

                hardwareDescription.Language.ShouldBe("VHDL");
                hardwareDescription.HardwareMembers.Count().ShouldBe(18);
                hardwareDescription.VhdlSource.ShouldNotBeNullOrEmpty();
            });
        }

        [Test]
        public async Task BasicVhdlStructureIsCorrect()
        {
            await _host.Run<ITransformer, IVhdlTransformationEventHandler>(async (transformer, eventHandler) =>
            {
                var topModule = (await TransformReferenceAssembliesToVhdl(transformer)).VhdlManifestIfFresh.TopModule;

                topModule.Entity.Name.ShouldNotBeNullOrEmpty();
                topModule.Entity.ShouldBe(topModule.Architecture.Entity, "The top module's entity is not referenced by the architecture.");

                Should.NotThrow(() =>
                    topModule.Architecture.Body.ShouldRecursivelyContain<Process>(p => p.Name.Contains("ExternalInvocationProxy")));
            });
        }


        private async Task<VhdlHardwareDescription> TransformReferenceAssembliesToVhdl(ITransformer transformer)
        {
            var configuration = new HardwareGenerationConfiguration { EnableCaching = false };
            configuration.TransformerConfiguration().UseSimpleMemory = false;
            return (VhdlHardwareDescription)await transformer.Transform(new[] { typeof(RootClass).Assembly, typeof(StaticReference).Assembly }, configuration);
        }


        private class StubMemberSuitabilityChecker : IMemberSuitabilityChecker
        {
            public bool IsSuitableInterfaceMember(EntityDeclaration member, ITypeDeclarationLookupTable typeDeclarationLookupTable)
            {
                return true;
            }
        }
    }
}
