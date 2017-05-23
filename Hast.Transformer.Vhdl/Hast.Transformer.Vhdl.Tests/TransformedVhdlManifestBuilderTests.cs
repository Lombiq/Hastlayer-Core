﻿using System;
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
using Orchard.Tests.Utility;
using Hast.VhdlBuilder.Extensions;
using Hast.Transformer.Services;
using Hast.Transformer.Vhdl.Services;
using Hast.Common.Models;
using Hast.Transformer.Vhdl.Events;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.InvocationProxyBuilders;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class TransformedVhdlManifestBuilderTests
    {
        private IContainer _container;

        private ITransformer _transformer;
        private TransformedVhdlManifestContainingVhdlTransformationEventHandler _eventHandler;


        [SetUp]
        public virtual void Init()
        {
            var builder = new ContainerBuilder();


            builder.RegisterAutoMocking(MockBehavior.Loose);

            // Although it's not ideal that we use DefaultTransformer and all the related pipeline here, the alternate would be to build a
            // syntax tree by hand, which is... Painful.

            builder.RegisterType<MethodTransformer>().As<IMethodTransformer>();
            builder.RegisterType<TypeConverter>().As<ITypeConverter>();
            builder.RegisterType<StatementTransformer>().As<IStatementTransformer>();
            builder.RegisterType<ExpressionTransformer>().As<IExpressionTransformer>();
            builder.RegisterType<MemberStateMachineFactory>().As<IMemberStateMachineFactory>();
            builder.RegisterType<ExternalInvocationProxyBuilder>().As<IExternalInvocationProxyBuilder>();
            builder.RegisterType<TransformedVhdlManifestBuilder>().As<ITransformedVhdlManifestBuilder>();
            builder.RegisterType<VhdlTransformingEngine>().As<ITransformingEngine>();
            _eventHandler = new TransformedVhdlManifestContainingVhdlTransformationEventHandler();
            builder.RegisterInstance(_eventHandler).As<IVhdlTransformationEventHandler>();
            builder.RegisterType<SyntaxTreeCleaner>().As<ISyntaxTreeCleaner>();
            builder.RegisterType<TypeDeclarationLookupTableFactory>().As<ITypeDeclarationLookupTableFactory>();
            builder.RegisterType<MemberSuitabilityChecker>().As<IMemberSuitabilityChecker>();
            builder.RegisterType<DefaultTransformer>().As<ITransformer>();

            _container = builder.Build();

            _transformer = _container.Resolve<ITransformer>();
        }

        [TestFixtureTearDown]
        public void Clean()
        {
        }


        [Test]
        public async Task BasicHardwareDescriptionPropertiesAreCorrect()
        {
            var hardwareDescription = await TransformReferenceAssembliesToVhdl();

            Assert.AreEqual(hardwareDescription.Language, "VHDL", "The language of the hardware description wasn't properly set to VHDL");
            // While there are 7 suitable methods the MemberIdTable will also include name alternates.
            Assert.AreEqual(hardwareDescription.MemberIdTable.Values.Count(), 11, "Not the proper amount of interface members were produced.");
        }

        [Test]
        public async Task BasicVhdlStructureIsCorrect()
        {
            var topModule = await TransformReferenceAssembliesAndGetTopModule();

            Assert.IsNotNullOrEmpty(topModule.Entity.Name, "The top module's entity doesn't have a name.");
            Assert.AreEqual(topModule.Entity, topModule.Architecture.Entity, "The top module's entity is not references by the architecture.");
            var callProxyProcess = topModule.Architecture.Body
                .SingleOrDefault(element => element is Process && ((Process)element).Name == "ExternalCallProxy".ToExtendedVhdlId());
            Assert.That(callProxyProcess != null, "There is no call proxy process.");
        }


        private async Task<VhdlBuilder.Representation.Declaration.Module> TransformReferenceAssembliesAndGetTopModule()
        {
            await TransformReferenceAssembliesToVhdl();
            return _eventHandler.TransformedVhdlManifest.Manifest.TopModule;
        }

        private async Task<VhdlHardwareDescription> TransformReferenceAssembliesToVhdl()
        {
            var configuration = new HardwareGenerationConfiguration { EnableCaching = false };
            configuration.TransformerConfiguration().UseSimpleMemory = false;
            return (VhdlHardwareDescription)await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, configuration);
        }


        private class TransformedVhdlManifestContainingVhdlTransformationEventHandler : IVhdlTransformationEventHandler
        {
            public ITransformedVhdlManifest TransformedVhdlManifest { get; set; }


            public void TransformedVhdlManifestBuilt(ITransformedVhdlManifest transformedVhdlManifest)
            {
                TransformedVhdlManifest = transformedVhdlManifest;
            }
        }
    }
}