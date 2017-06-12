using System;
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
        public static bool IsOutFlowing(this ParameterDeclaration parameter) =>
            // If the parameter is a value type then still it need to be out-flowing if this is a constructor.
            (!parameter.Annotation<ParameterDefinition>().ParameterType.IsValueType || 
                (parameter.FindFirstParentEntityDeclaration().GetFullName().IsConstructorName() && 
                parameter.FindFirstParentTypeDeclaration().GetFullName() == parameter.Annotation<ParameterDefinition>().ParameterType.FullName)) ||
            parameter.ParameterModifier.HasFlag(ParameterModifier.Out) ||
            parameter.ParameterModifier.HasFlag(ParameterModifier.Ref);
    }
}
