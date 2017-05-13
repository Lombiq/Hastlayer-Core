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
using Hast.VhdlBuilder.Testing;
using Hast.VhdlBuilder.Representation;
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
            return (VhdlHardwareDescription)await transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, configuration);
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
