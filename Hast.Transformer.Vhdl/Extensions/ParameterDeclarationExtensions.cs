using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class ParameterDeclarationExtensions
    {
        /// <summary>
        /// Determines whether the parameter has and "out-flowing" characteristic, i.e. changes to it inside the parent
        /// method should be reflected in the argument passed in too. A parameter is out-flowing if it contains a 
        /// reference type or is explicitly passed by reference, or if it's an out parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static bool IsOutFlowing(this ParameterDeclaration parameter)
        {
            return 
                !parameter.Annotation<ParameterDefinition>().ParameterType.IsValueType ||
                parameter.ParameterModifier.HasFlag(ParameterModifier.Out) ||
                parameter.ParameterModifier.HasFlag(ParameterModifier.Ref);
        }
    }
}
