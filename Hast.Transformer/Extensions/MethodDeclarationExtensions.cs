using System;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class MethodDeclarationExtensions
    {
        /// <summary>
        /// Finds the method in an interface that the declaration implements a method of, if any.
        /// </summary>
        /// <returns>
        /// The <see cref="TypeDeclaration"/> of the interface's method that the declaration implements a method of, 
        /// or <c>null</c> if the declaration is not an implementation of any method of any interface.
        /// </returns>
        public static MethodDeclaration FindImplementedInterfaceMethod(this MethodDeclaration declaration, Func<AstType, TypeDeclaration> lookupDeclaration)
        {
            // This is an explicitly implemented method so just returning the interface's type declaration directly.
            if (!declaration.PrivateImplementationType.IsNull)
            {
                var interfaceDeclaration = lookupDeclaration(declaration.PrivateImplementationType);
                if (interfaceDeclaration != null)
                {
                    return interfaceDeclaration.FindMatchingMethod(declaration, lookupDeclaration);
                }
                return null;
            }

            // Otherwise if it's not public it can't be a member declared in an interface.
            if (declaration.Modifiers != Modifiers.Public) return null;

            // Searching for an implemented interface with the same member.
            var parent = (TypeDeclaration)declaration.Parent;
            foreach (var baseType in parent.BaseTypes) // BaseTypes are flattened, so interface inheritance is taken into account.
            {
                if (baseType.NodeType == NodeType.TypeReference)
                {
                    // baseType is a TypeReference but we need the corresponding TypeDeclaration to check for the methods.
                    var baseTypeDeclaration = lookupDeclaration(baseType);

                    if (baseTypeDeclaration.ClassType == ClassType.Interface)
                    {
                        var matchingMethod = baseTypeDeclaration.FindMatchingMethod(declaration, lookupDeclaration);
                        if (matchingMethod != null) return matchingMethod;
                    }
                }
            }

            return null;
        }
    }
}
