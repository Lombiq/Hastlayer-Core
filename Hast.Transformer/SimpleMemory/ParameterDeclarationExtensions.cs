using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class ParameterDeclarationExtensions
    {
        public static bool IsSimpleMemoryParameter(this ParameterDeclaration parameterDeclaration)
        {
            var type = parameterDeclaration.Type as SimpleType;
            if (type == null) return false;
            return type.Identifier == "SimpleMemory";
        }
    }
}
