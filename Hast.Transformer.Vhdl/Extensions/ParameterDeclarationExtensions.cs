using System;
using System.Linq;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class ParameterDeclarationExtensions
    {
        /// <summary>
        /// Determines whether the parameter has an "out-flowing" characteristic, i.e. changes to it inside the parent
        /// method should be reflected in the argument passed in too. A parameter is out-flowing if it contains a 
        /// reference type or is explicitly passed by reference, or if it's an out parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsOutFlowing(
            this ParameterDeclaration parameter,
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var parameterDefinition = parameter.Annotation<ParameterDefinition>();

            var typeHasReferenceTypeMembers = false;
            var typeDeclaration = typeDeclarationLookupTable.Lookup(parameterDefinition.ParameterType.FullName);
            if (typeDeclaration != null)
            {
                foreach (var member in typeDeclaration.Members
                        .Where(member => member is PropertyDeclaration || member is FieldDeclaration))
                {
                    if (!typeHasReferenceTypeMembers)
                    {
                        typeHasReferenceTypeMembers = !(member.ReturnType is PrimitiveType);
                    }
                }
             }

            // If the parameter is a value type then still it need to be out-flowing if this is a constructor. It also
            // needs to be out-flowing if it contains reference types.
            return 
            (!parameterDefinition.ParameterType.IsValueType ||
                typeHasReferenceTypeMembers ||
                (parameter.FindFirstParentEntityDeclaration().GetFullName().IsConstructorName() &&
                parameter.FindFirstParentTypeDeclaration().GetFullName() == parameterDefinition.ParameterType.FullName)) ||
            parameter.ParameterModifier.HasFlag(ParameterModifier.Out) ||
            parameter.ParameterModifier.HasFlag(ParameterModifier.Ref);
        }
    }
}
