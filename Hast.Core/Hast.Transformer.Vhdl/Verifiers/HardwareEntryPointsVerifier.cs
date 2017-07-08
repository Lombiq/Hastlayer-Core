using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.Verifiers
{
    public class HardwareEntryPointsVerifier : IHardwareEntryPointsVerifier
    {
        private readonly IMemberSuitabilityChecker _memberSuitabilityChecker;


        public HardwareEntryPointsVerifier(IMemberSuitabilityChecker memberSuitabilityChecker)
        {
            _memberSuitabilityChecker = memberSuitabilityChecker;
        }


        public void VerifyHardwareEntryPoints(SyntaxTree syntaxTree, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var hardwareEntryPointTypes = syntaxTree
                .GetTypes()
                .Where(entity =>
                    entity.Is<TypeDeclaration>(type =>
                        type.Members.Any(member =>
                            _memberSuitabilityChecker.IsSuitableHardwareEntryPointMember(member, typeDeclarationLookupTable))))
                .Cast<TypeDeclaration>();

            if (hardwareEntryPointTypes.Any(type => 
                type.Members.Any(member => member is FieldDeclaration || member is PropertyDeclaration || member.GetFullName().IsConstructorName())))
            {
                throw new NotSupportedException("Fields, properties and constructors are not supported in hardware entry point types.");
            }
        }
    }
}
