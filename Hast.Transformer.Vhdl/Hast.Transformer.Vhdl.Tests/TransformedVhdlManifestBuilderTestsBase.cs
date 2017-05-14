using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.Tests.IntegrationTestingServices;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.Tests
{
    public abstract class TransformedVhdlManifestBuilderTestsBase : IntegrationTestsBase
    {
        protected TransformedVhdlManifestBuilderTestsBase()
        {
            _requiredExtension.AddRange(new[] { typeof(ITransformer).Assembly, typeof(MemberIdTable).Assembly });

            _shellRegistrationBuilder = builder =>
            {
                builder.RegisterInstance(new StubMemberSuitabilityChecker()).As<IMemberSuitabilityChecker>();
            };
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
