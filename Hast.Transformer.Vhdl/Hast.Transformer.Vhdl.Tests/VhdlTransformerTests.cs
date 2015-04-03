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
using Hast.Transformer.Events;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.SubTransformers;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Moq;
using NUnit.Framework;
using Orchard.Tests.Utility;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class VhdlTransformerTests
    {
        private IContainer _container;

        private ITransformer _transformer;
        private TransformationContextContainingTransformerEventHandler _eventHandler;


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
            builder.RegisterType<VhdlTransformingEngine>().As<ITransformingEngine>();
            _eventHandler = new TransformationContextContainingTransformerEventHandler();
            builder.RegisterInstance(_eventHandler).As<ITransformerEventHandler>();
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
            Assert.AreEqual(hardwareDescription.MethodIdTable.Values.Count(), 7, "Not the proper amount of interface members were produced.");
        }

        [Test]
        public async Task BasicVhdlStructureIsCorrect()
        {
            var topModule = await TransformReferenceAssembliesAndGetTopModule();

            Assert.IsNotNullOrEmpty(topModule.Entity.Name, "The top module's entity doesn't have a name.");
            Assert.AreEqual(topModule.Entity, topModule.Architecture.Entity, "The top module's entity is not references by the architecture.");
            Assert.AreEqual(topModule.Architecture.Body.Count, 1, "There is not just one element, the call proxy, in the top module's architecture's body.");
            var callProxyProcess = topModule.Architecture.Body.Single();
            Assert.That(callProxyProcess is Process && ((Process)callProxyProcess).Name == "CallProxy", "There is no call proxy process.");
        }

        [Test]
        public async Task CallProxyWellStructured()
        {
            var topModule = await TransformReferenceAssembliesAndGetTopModule();

            Assert.That(topModule.Architecture.Body.Count == 1 &&
                topModule.Architecture.Body.Single() is Process &&
                ((Process)topModule.Architecture.Body.Single()).Body.Count == 1 &&
                ((Process)topModule.Architecture.Body.Single()).Body.Single() is IfElse, "The structure of the call proxy is not correct.");

            var ifElseElement = (IfElse)((Process)topModule.Architecture.Body.Single()).Body.Single();

            Assert.That(ifElseElement.TrueElements.Count == 1 && ifElseElement.TrueElements.Single() is Case, "The structure of the call proxy's inner branching is not correct.");

            var caseElement = (Case)ifElseElement.TrueElements.Single();
            var memberSuitabilityChecker = _container.Resolve<IMemberSuitabilityChecker>();
            var interfaceMethodCount = _eventHandler.TransformationContext.SyntaxTree
                .GetTypes(true)
                .Sum(type => type.Members.Count(member => memberSuitabilityChecker.IsSuitableInterfaceMember(member, _eventHandler.TransformationContext.TypeDeclarationLookupTable)));
            // Removing 1 because there is always +1 when for "others".
            Assert.AreEqual(caseElement.Whens.Count - 1, interfaceMethodCount, "There are not enough cases in the call proxy process for every interface member.");
        }


        private async Task<Hast.VhdlBuilder.Representation.Declaration.Module> TransformReferenceAssembliesAndGetTopModule()
        {
            return (await TransformReferenceAssembliesToVhdl()).Manifest.TopModule;
        }

        private async Task<VhdlHardwareDescription> TransformReferenceAssembliesToVhdl()
        {
            return (VhdlHardwareDescription)await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, HardwareGenerationConfiguration.Default);
        }


        private class TransformationContextContainingTransformerEventHandler : ITransformerEventHandler
        {
            public ITransformationContext TransformationContext { get; set; }


            public void SyntaxTreeBuilt(ITransformationContext transformationContext)
            {
                TransformationContext = transformationContext;
            }
        }
    }
}
