using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Communication;
using Hast.Tests.TestAssembly1;
using Hast.Tests.TestAssembly2;
using Hast.Transformer.Extensibility.Events;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Moq;
using NUnit.Framework;
using Hast.VhdlBuilder.Extensions;
using Hast.Transformer.Services;
using Hast.Transformer.Vhdl.Services;
using Hast.Common.Models;
using Hast.Transformer.Vhdl.Events;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.InvocationProxyBuilders;
using Orchard.Environment.Configuration;
using Lombiq.OrchardAppHost;
using Lombiq.OrchardAppHost.Configuration;
using System.Reflection;
using Orchard.Events;
using Autofac.Core;
using Orchard.Tests.Utility;
using Orchard.Environment;
using Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class TransformedVhdlManifestBuilderTests : IntegrationTestBase
    {
        private TransformedVhdlManifestContainingVhdlTransformationEventHandler _eventHandler;


        public TransformedVhdlManifestBuilderTests()
        {
            _requiredExtension.AddRange(new[] { typeof(ITransformer).Assembly, typeof(MemberIdTable).Assembly });

            _shellRegistrationBuilder = builder =>
            {
                _eventHandler = new TransformedVhdlManifestContainingVhdlTransformationEventHandler();
                builder.RegisterInstance(_eventHandler).As<IVhdlTransformationEventHandler>();
                builder.RegisterInstance(new StubMemberSuitabilityChecker()).As<IMemberSuitabilityChecker>();
            };
        }


        [Test]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            await _host.Run<ITransformer>(async transformer =>
            {
                var hardwareDescription = await TransformReferenceAssembliesToVhdl(transformer);

                Assert.AreEqual(hardwareDescription.Language, "VHDL", "The language of the hardware description wasn't properly set to VHDL");
                Assert.AreEqual(hardwareDescription.HardwareMembers.Count(), 18, "Not the proper amount of hardware members were produced.");
                Assert.IsNotEmpty(hardwareDescription.VhdlSource, "The VHDL source was empty.");
            });
        }

        [Test]
        public async Task BasicVhdlStructureIsCorrect()
        {
            await _host.Run<ITransformer, IVhdlTransformationEventHandler>(async (transformer, eventHandler) =>
            {
                var topModule = await TransformReferenceAssembliesAndGetTopModule(transformer);

                Assert.That(topModule.Entity.Name, Is.Not.Null.Or.Empty, "The top module's entity doesn't have a name.");
                Assert.AreEqual(topModule.Entity, topModule.Architecture.Entity, "The top module's entity is not references by the architecture.");
                var callProxyProcess = topModule.Architecture.Body
                    .SingleOrDefault(element => element is Process && ((Process)element).Name == "ExternalCallProxy".ToExtendedVhdlId());
                Assert.That(callProxyProcess != null, "There is no call proxy process.");
            });
        }


        private async Task<VhdlBuilder.Representation.Declaration.Module> TransformReferenceAssembliesAndGetTopModule(
            ITransformer transformer)
        {
            await TransformReferenceAssembliesToVhdl(transformer);
            // This will only work if a single  test runs at once.
            return _eventHandler.TransformedVhdlManifest.Manifest.TopModule;
        }

        private async Task<VhdlHardwareDescription> TransformReferenceAssembliesToVhdl(ITransformer transformer)
        {
            var configuration = new HardwareGenerationConfiguration { EnableCaching = false };
            configuration.TransformerConfiguration().UseSimpleMemory = false;
            return (VhdlHardwareDescription)await transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, configuration);
        }


        private class TransformedVhdlManifestContainingVhdlTransformationEventHandler : IVhdlTransformationEventHandler
        {
            public ITransformedVhdlManifest TransformedVhdlManifest { get; set; }


            public void TransformedVhdlManifestBuilt(ITransformedVhdlManifest transformedVhdlManifest)
            {
                TransformedVhdlManifest = transformedVhdlManifest;
            }
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
