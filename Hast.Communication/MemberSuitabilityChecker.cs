using Hast.Transformer;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Communication
{
    // The implementation is here because it depends on the Communication component and its proxy generator on what can 
    // be used as hardware entry point members.
    public class MemberSuitabilityChecker : IMemberSuitabilityChecker
    {
        public bool IsSuitableHardwareEntryPointMember(EntityDeclaration member, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            if (member is MethodDeclaration)
            {
                var method = (MethodDeclaration)member;

                if (method.Parent is TypeDeclaration &&
                        // If it's a public virtual method,
                        (method.Modifiers == (Modifiers.Public | Modifiers.Virtual) ||
                        // or a public virtual async method,
                        method.Modifiers == (Modifiers.Public | Modifiers.Virtual | Modifiers.Async) ||
                        // or a public method that implements an interface.
                        method.FindImplementedInterfaceMethod(typeDeclarationLookupTable.Lookup) != null))
                {
                    var parent = (TypeDeclaration)method.Parent;
                    return parent.ClassType == ClassType.Class && parent.Modifiers == Modifiers.Public;
                }
            }

            return false;
        }
    }
}
