﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Communication;
using Hast.Tests.TestAssembly1;
using Hast.Tests.TestAssembly1.ComplexTypes;
using Hast.Tests.TestAssembly2;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Moq;
using NUnit.Framework;
using Orchard.Tests.Utility;

namespace Hast.Transformer.Vhdl.Tests
{
    [TestFixture]
    public class TransformerTests
    {
        private IContainer _container;

        private ITransformer _transformer;
        private ITransformationContext _producedContext;
        private Mock<ITransformingEngine> _transformingEngineMock;


        [SetUp]
        public virtual void Init()
        {
            var builder = new ContainerBuilder();


            builder.RegisterAutoMocking(MockBehavior.Loose);

            builder.RegisterType<SyntaxTreeCleaner>().As<ISyntaxTreeCleaner>();
            builder.RegisterType<TypeDeclarationLookupTableFactory>().As<ITypeDeclarationLookupTableFactory>();
            builder.RegisterType<MemberSuitabilityChecker>().As<IMemberSuitabilityChecker>();
            builder.RegisterType<SyntaxTreeCleaner>().As<ISyntaxTreeCleaner>();

            _transformingEngineMock = new Mock<ITransformingEngine>();

            _transformingEngineMock
                .Setup(engine => engine.Transform(It.IsAny<ITransformationContext>()))
                .Returns<ITransformationContext>(context =>
                    {
                        // Sending out the context through a field is not a nice solutions but there doesn't seem to be a better one.
                        _producedContext = context;
                        return Task.FromResult<IHardwareDescription>(null);
                    })
                .Verifiable();
            builder.RegisterInstance(_transformingEngineMock.Object).As<ITransformingEngine>();


            builder.RegisterType<DefaultTransformer>().As<ITransformer>();

            _container = builder.Build();

            _transformer = _container.Resolve<ITransformer>();
        }

        [TestFixtureTearDown]
        public void Clean()
        {
        }


        [Test]
        public async Task TransformEngineCallReceivesProperBasicContext()
        {
            var configuration = HardwareGenerationConfiguration.Default;

            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, configuration);

            _transformingEngineMock.Verify(engine => engine.Transform(It.Is<ITransformationContext>(context => context != null)));
            Assert.IsNotNullOrEmpty(_producedContext.Id, "The ID for the transformation context was not properly set.");
            Assert.NotNull(_producedContext.SyntaxTree, "No syntax tree tree was set for the transformation context.");
            Assert.AreEqual(configuration, _producedContext.HardwareGenerationConfiguration, "The input hardware generation configuration was not properly passed on to the transformation context.");
            Assert.NotNull(_producedContext.TypeDeclarationLookupTable, "No type declaration lookup table was set for the transformation context.");
        }

        [Test]
        public async Task DifferentConfigurationsResultInDifferentIds()
        {
            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, HardwareGenerationConfiguration.Default);
            var firstId = _producedContext.Id;
            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, HardwareGenerationConfiguration.Default);
            Assert.AreNotEqual(firstId, _producedContext.Id, "The transformation context ID isn't different despite the set of assemblies transformed being different.");

            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, new HardwareGenerationConfiguration { MaxDegreeOfParallelism = 5 });
            firstId = _producedContext.Id;
            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, new HardwareGenerationConfiguration { MaxDegreeOfParallelism = 15 });
            Assert.AreNotEqual(firstId, _producedContext.Id, "The transformation context ID isn't different despite the max degree of parallelism being different.");

            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, new HardwareGenerationConfiguration { IncludedMembers = new[] { "aaa" } });
            firstId = _producedContext.Id;
            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, new HardwareGenerationConfiguration { IncludedMembers = new[] { "bbb" } });
            Assert.AreNotEqual(firstId, _producedContext.Id, "The transformation context ID isn't different despite the set of included members being different.");

            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, new HardwareGenerationConfiguration { IncludeMembersPrefixed = new[] { "aaa" } });
            firstId = _producedContext.Id;
            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly }, new HardwareGenerationConfiguration { IncludeMembersPrefixed = new[] { "bbb" } });
            Assert.AreNotEqual(firstId, _producedContext.Id, "The transformation context ID isn't different despite the set of included members prefixed being different.");
        }

        [Test]
        public async Task UnusedDeclarationsArentInTheSyntaxTree()
        {
            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, HardwareGenerationConfiguration.Default);
            var typeLookup = BuildTypeLookup();

            Assert.IsFalse(typeLookup.ContainsKey(typeof(UnusedDeclarations).Name), "Classes with unreferenced members weren't removed from the syntax tree.");
            Assert.IsFalse(typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Any(member => member.Name == "UnusedMethod"), "Unreferenced members of classes weren't removed from the syntax tree.");
        }

        [Test]
        public async Task IncludedMembersAndTheirReferencesAreOnlyInTheSyntaxTree()
        {
            var configuration = new HardwareGenerationConfiguration
            {
                IncludedMembers = new[]
                {
                    "System.Boolean Hast.Tests.TestAssembly1.ComplexAlgorithm::IsPrimeNumber(System.Int32)",
                    "System.Int32 Hast.Tests.TestAssembly1.ComplexTypes.ComplexTypeHierarchy::Hast.Tests.TestAssembly1.ComplexTypes.IInterface1.Interface1Method1()"
                }
            };

            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, configuration);
            var typeLookup = BuildTypeLookup();

            Assert.AreEqual(typeLookup.Count, 5, "Not the number of types remained in the syntax tree than there are used.");
            Assert.AreEqual(typeLookup[typeof(ComplexAlgorithm).Name].Members.Count, 1);
            Assert.AreEqual(typeLookup[typeof(ComplexAlgorithm).Name].Members.Single().Name, "IsPrimeNumber");
            Assert.AreEqual(typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Count, 3);
            Assert.IsTrue(typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Select(member => member.Name)
                .SequenceEqual(new[]{ "Interface1Method1", "PrivateMethod", "StaticMethod" }));
        }

        [Test]
        public async Task IncludedMembersPrefixedAndTheirReferencesAreOnlyInTheSyntaxTree()
        {
            var configuration = new HardwareGenerationConfiguration
            {
                IncludeMembersPrefixed = new[]
                {
                    "Hast.Tests.TestAssembly1.ComplexAlgorithm.IsPrimeNumber",
                    "Hast.Tests.TestAssembly1.ComplexTypes"
                }
            };

            await _transformer.Transform(new[] { typeof(ComplexAlgorithm).Assembly, typeof(StaticReference).Assembly }, configuration);
            var typeLookup = BuildTypeLookup();

            Assert.AreEqual(typeLookup.Count, 6, "Not the number of types remained in the syntax tree than there are used.");
            Assert.AreEqual(typeLookup[typeof(ComplexAlgorithm).Name].Members.Count, 1);
            Assert.AreEqual(typeLookup[typeof(ComplexAlgorithm).Name].Members.Single().Name, "IsPrimeNumber");
            Assert.AreEqual(typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Count, 7);
            Assert.IsTrue(typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Select(member => member.Name)
                .SequenceEqual(new[] { "Interface1Method1", "Interface1Method2", "Interface2Method1", "BaseInterfaceMethod1", "BaseInterfaceMethod2", "PrivateMethod", "StaticMethod" }));
        }

        
        private Dictionary<string, TypeDeclaration> BuildTypeLookup()
        {
            return _producedContext.SyntaxTree.GetTypes(true).ToDictionary(type => type.Name);
        }
    }
}
