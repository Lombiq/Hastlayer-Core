using Autofac;
using Hast.Layer;
using Hast.Synthesis;
using Hast.Synthesis.Services;
using Hast.TestInputs.ClassStructure1;
using Hast.TestInputs.ClassStructure1.ComplexTypes;
using Hast.TestInputs.ClassStructure2;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Configuration;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using Hast.Xilinx;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Xilinx.Abstractions;
using Moq;
using NUnit.Framework;
using Orchard.Services;
using Orchard.Tests.Utility;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            builder.RegisterType<DefaultJsonConverter>().As<IJsonConverter>();
            builder.RegisterType<SyntaxTreeCleaner>().As<ISyntaxTreeCleaner>();
            builder.RegisterType<TypeDeclarationLookupTableFactory>().As<ITypeDeclarationLookupTableFactory>();
            builder.RegisterType<MemberSuitabilityChecker>().As<IMemberSuitabilityChecker>();
            builder.RegisterType<DeviceDriverSelector>().As<IDeviceDriverSelector>();
            builder.RegisterType<Nexys4DdrDriver>().As<IDeviceDriver>();

            _transformingEngineMock = new Mock<ITransformingEngine>();

            _transformingEngineMock
                .Setup(engine => engine.Transform(It.IsAny<ITransformationContext>()))
                .Returns<ITransformationContext>(context =>
                    {
                        // Sending out the context through a field is not a nice solutions but there doesn't seem to be 
                        // a better one.
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
            var configuration = CreateConfig();

            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, configuration);

            _transformingEngineMock.Verify(engine => engine.Transform(It.Is<ITransformationContext>(context => context != null)));

            _producedContext.Id.ShouldNotBeNullOrEmpty();
            _producedContext.SyntaxTree.ShouldNotBeNull();
            configuration.ShouldBe(_producedContext.HardwareGenerationConfiguration, "The input hardware generation configuration was not properly passed on to the transformation context.");
            _producedContext.TypeDeclarationLookupTable.ShouldNotBeNull();
        }

        [Test]
        public async Task DifferentConfigurationsResultInDifferentIds()
        {
            var config = CreateConfig();
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            var firstId = _producedContext.Id;
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly, typeof(StaticReference).Assembly }, config);
            firstId.ShouldNotBe(_producedContext.Id, "The transformation context ID isn't different despite the set of assemblies transformed being different.");


            config.TransformerConfiguration().AddMemberInvocationInstanceCountConfiguration(
                new MemberInvocationInstanceCountConfiguration("Hast.TestInputs.ClassStructure1.RootClass.VirtualMethod")
                {
                    MaxDegreeOfParallelism = 5
                });
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            firstId = _producedContext.Id;
            config.TransformerConfiguration().MemberInvocationInstanceCountConfigurations.Single().MaxDegreeOfParallelism = 15;
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            firstId.ShouldNotBe(_producedContext.Id, "The transformation context ID isn't different despite the max degree of parallelism being different.");

            config.HardwareEntryPointMemberFullNames = new[] { "aaa" };
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            firstId = _producedContext.Id;
            config.HardwareEntryPointMemberFullNames = new[] { "bbb" };
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            firstId.ShouldNotBe(_producedContext.Id, "The transformation context ID isn't different despite the set of included members being different.");

            config.HardwareEntryPointMemberNamePrefixes = new[] { "aaa" };
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            firstId = _producedContext.Id;
            config.HardwareEntryPointMemberNamePrefixes = new[] { "bbb" };
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly }, config);
            firstId.ShouldNotBe(_producedContext.Id, "The transformation context ID isn't different despite the set of included members prefixed being different.");
        }

        [Test]
        public async Task UnusedDeclarationsArentInTheSyntaxTree()
        {
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly, typeof(StaticReference).Assembly }, CreateConfig());
            var typeLookup = BuildTypeLookup();

            typeLookup.Count.ShouldBe(7, "Not the number of types remained in the syntax tree than there are used.");
            typeLookup.ShouldNotContainKey(typeof(UnusedDeclarations).Name, "Classes with unreferenced members weren't removed from the syntax tree.");
            typeLookup[typeof(ComplexTypeHierarchy).Name].Members.ShouldNotContain(
                member => member.Name == "UnusedMethod" || member.Name == "NonVirtualNonInterfaceMehod",
                "Unreferenced members of classes weren't removed from the syntax tree.");
        }

        [Test]
        public async Task IncludedMembersAndTheirReferencesAreOnlyInTheSyntaxTree()
        {
            var configuration = CreateConfig();
            configuration.HardwareEntryPointMemberFullNames = new[]
            {
                "System.Void " + typeof(RootClass).FullName + "::" + nameof(RootClass.VirtualMethod) + "(System.Int32)",
                "System.Void " + typeof(ComplexTypeHierarchy).FullName + "::" + typeof(IInterface1).FullName + "." + nameof(IInterface1.Interface1Method1) + "()"
            };

            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly, typeof(StaticReference).Assembly }, configuration);
            var typeLookup = BuildTypeLookup();

            typeLookup.Count.ShouldBe(3, "Not the number of types remained in the syntax tree than there are used.");
            typeLookup[typeof(RootClass).Name].Members.Count.ShouldBe(1);
            typeLookup[typeof(RootClass).Name].Members.Single().Name.ShouldBe("VirtualMethod");
            typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Count.ShouldBe(3);
            typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Select(member => member.Name)
                .SequenceEqual(new[] { "Interface1Method1", "PrivateMethod", "StaticMethod" })
                .ShouldBeTrue();
            typeLookup[typeof(IInterface1).Name].Members.Count.ShouldBe(1);
            typeLookup[typeof(IInterface1).Name].Members.Select(member => member.Name)
                .SequenceEqual(new[] { "Interface1Method1" })
                .ShouldBeTrue();
        }

        [Test]
        public async Task IncludedMembersPrefixedAndTheirReferencesAreOnlyInTheSyntaxTree()
        {
            var configuration = CreateConfig();
            configuration.HardwareEntryPointMemberNamePrefixes = new[]
            {
                typeof(RootClass).FullName + "." + nameof(RootClass.VirtualMethod),
                typeof(ComplexTypeHierarchy).Namespace
            };
            await _transformer.Transform(new[] { typeof(ComplexTypeHierarchy).Assembly, typeof(StaticReference).Assembly }, configuration);
            var typeLookup = BuildTypeLookup();

            typeLookup.Count.ShouldBe(5, "Not the number of types remained in the syntax tree than there are used.");
            typeLookup[typeof(RootClass).Name].Members.Count.ShouldBe(1);
            typeLookup[typeof(RootClass).Name].Members.Single().Name.ShouldBe("VirtualMethod");
            typeLookup[typeof(ComplexTypeHierarchy).Name].Members.Count.ShouldBe(7);
            typeLookup[typeof(ComplexTypeHierarchy).Name].Members
                .Select(member => member.Name)
                .SequenceEqual(new[] { "Interface1Method1", "Interface1Method2", "Interface2Method1", "BaseInterfaceMethod1", "BaseInterfaceMethod2", "PrivateMethod", "StaticMethod" })
                .ShouldBeTrue();
        }


        private Dictionary<string, TypeDeclaration> BuildTypeLookup()
        {
            return _producedContext.SyntaxTree.GetAllTypeDeclarations().ToDictionary(type => type.Name);
        }


        private static HardwareGenerationConfiguration CreateConfig()
        {
            var configuration = new HardwareGenerationConfiguration(Nexys4DdrManifestProvider.DeviceName);
            configuration.TransformerConfiguration().UseSimpleMemory = false;
            return configuration;
        }
    }
}
