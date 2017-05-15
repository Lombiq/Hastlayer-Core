using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Hast.Common.Configuration;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.Tests
{
    public abstract class VhdlTransformingTestFixtureBase : IntegrationTestFixtureBase
    {
        protected VhdlTransformingTestFixtureBase()
        {
            _requiredExtension.AddRange(new[] { typeof(ITransformer).Assembly, typeof(MemberIdTable).Assembly });

            _shellRegistrationBuilder = builder =>
            {
                builder.RegisterInstance(new StubMemberSuitabilityChecker()).As<IMemberSuitabilityChecker>();
            };
        }


        protected virtual async Task<VhdlHardwareDescription> TransformAssembliesToVhdl(
            ITransformer transformer,
            IEnumerable<Assembly> assemblies,
            Action<HardwareGenerationConfiguration> configurationModifier = null)
        {
            var configuration = new HardwareGenerationConfiguration { EnableCaching = false };
            configurationModifier?.Invoke(configuration);
            return (VhdlHardwareDescription)await transformer.Transform(assemblies, configuration);
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
