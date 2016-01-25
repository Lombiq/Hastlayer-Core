using System;
using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class TypeDeclarationExtensions
    {
        /// <summary>
        /// Searches for a method on the type that has the same signature as the supplied method.
        /// </summary>
        /// <returns>The declaration of the matching method if found, <c>null</c> otherwise.</returns>
        public static MethodDeclaration FindMatchingMethod(
            this TypeDeclaration typeDeclaration, 
            MethodDeclaration methodDeclaration, 
            Func<AstType, TypeDeclaration> lookupDeclaration)
        {
            // Searching for members that have the exact same signature.
            var matchedMember = typeDeclaration.Members.SingleOrDefault(member =>
            {
                if (member.Name == methodDeclaration.Name && member.EntityType == ICSharpCode.NRefactory.TypeSystem.EntityType.Method)
                {
                    var method = (MethodDeclaration)member;
                    if ((typeDeclaration.ClassType == ClassType.Interface || method.Modifiers == methodDeclaration.Modifiers) && // Only checking for modifiers is the type is not an interface.
                        method.ReturnType.TypeEquals(methodDeclaration.ReturnType, lookupDeclaration) &&
                        method.Parameters.Count == methodDeclaration.Parameters.Count)
                    {
                        foreach (var interfaceMethodParameter in method.Parameters)
                        {
                            if (!methodDeclaration.Parameters.Any(parameter => parameter.Type.TypeEquals(interfaceMethodParameter.Type, lookupDeclaration)))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }

                return false;
            });

            if (matchedMember != null) return (MethodDeclaration)matchedMember;
            return null;
        }
    }
}
