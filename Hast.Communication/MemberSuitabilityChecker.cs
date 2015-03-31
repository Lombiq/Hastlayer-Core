using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Communication
{
    // The implementation is here because it depends on the Communication component and its proxy generator what can be used as interface members.
    public class MemberSuitabilityChecker : IMemberSuitabilityChecker
    {
        public bool IsSuitableInterfaceMember(EntityDeclaration member, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            if (member is MethodDeclaration)
            {
                var method = (MethodDeclaration)member;

                if (method.Parent is TypeDeclaration &&
                    (method.Modifiers == (Modifiers.Public | Modifiers.Virtual) || // If it's a public virtual method,
                        IsInterfaceDeclaredMethod(method, typeDeclarationLookupTable))) // or a public method that implements an interface,
                {
                    var parent = (TypeDeclaration)method.Parent;
                    return parent.ClassType == ClassType.Class && parent.Modifiers == Modifiers.Public;
                } 
            }

            return false;
        }


        private static bool IsInterfaceDeclaredMethod(MethodDeclaration method, ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            // Is this an explicitly implemented interface method?
            if (method.Modifiers == Modifiers.None &&
                method.NameToken.NextSibling != null &&
                method.NameToken.NextSibling.NodeType == NodeType.TypeReference)
            {
                return true;
            }

            // Otherwise if it's not public it can't be a method declared in an interface (public virtuals are checked separately).
            if (method.Modifiers != Modifiers.Public) return false;

            // Searching for an implemented interface with the same method.
            var parent = (TypeDeclaration)method.Parent;
            foreach (var baseType in parent.BaseTypes) // BaseTypes are flattened, so interface inheritance is taken into account.
            {
                if (baseType.NodeType == NodeType.TypeReference)
                {
                    // baseType is a TypeReference but we need the corresponding TypeDeclaration to check for the methods.
                    var baseTypeDeclaration = typeDeclarationLookupTable.Lookup(baseType);

                    if (baseTypeDeclaration.ClassType == ClassType.Interface)
                    {
                        if (baseTypeDeclaration.Members.Any(member =>
                        {
                            if (member.Name == method.Name && member.EntityType == ICSharpCode.NRefactory.TypeSystem.EntityType.Method)
                            {
                                var interfaceMethod = (MethodDeclaration)member;
                                if (interfaceMethod.ReturnType.TypeEquals(method.ReturnType,typeDeclarationLookupTable.Lookup) &&
                                    interfaceMethod.Parameters.Count == method.Parameters.Count)
                                {
                                    foreach (var interfaceMethodParameter in interfaceMethod.Parameters)
                                    {
                                        if (!method.Parameters.Any(parameter => parameter.Type.TypeEquals(interfaceMethodParameter.Type, typeDeclarationLookupTable.Lookup)))
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                            }

                            return false;
                        }))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
