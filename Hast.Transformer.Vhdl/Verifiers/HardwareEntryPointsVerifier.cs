using System;
using System.Linq;
using Hast.Transformer.Models;
using Hast.Transformer.Services;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.Verifiers
{
    public class HardwareEntryPointsVerifier : IHardwareEntryPointsVerifier
    {
        private readonly IMemberSuitabilityChecker _memberSuitabilityChecker;

        public HardwareEntryPointsVerifier(IMemberSuitabilityChecker memberSuitabilityChecker) => _memberSuitabilityChecker = memberSuitabilityChecker;

        public void VerifyHardwareEntryPoints(SyntaxTree syntaxTree, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var hardwareEntryPointTypes = syntaxTree
                .GetTypes()
                .Where(entity =>
                    entity.Is<TypeDeclaration>(type =>
                        type.Members.Any(member =>
                            _memberSuitabilityChecker.IsSuitableHardwareEntryPointMember(member, typeDeclarationLookupTable))))
                .Cast<TypeDeclaration>();

            foreach (var type in hardwareEntryPointTypes)
            {
                var unsupportedMembers = type
                    .Members
                    .Where(member => member is FieldDeclaration || member is PropertyDeclaration || member.GetFullName().IsConstructorName());
                if (unsupportedMembers.Any())
                {
                    throw new NotSupportedException(
                        "Fields, properties and constructors are not supported in hardware entry point types. The type " +
                        type.GetFullName() + " contains the following unsupported members: " +
                        string.Join(", ", unsupportedMembers.Select(member => member.GetFullName())));
                }
            }
        }
    }
}
