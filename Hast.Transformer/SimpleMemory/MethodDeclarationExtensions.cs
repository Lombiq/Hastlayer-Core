using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class MethodDeclarationExtensions
    {
        public static string GetSimpleMemoryParameterName(this MethodDeclaration methodDeclaration)
        {
            var parameter = methodDeclaration.Parameters.SingleOrDefault(p => p.IsSimpleMemoryParameter());
            if (parameter == null) return null;
            return parameter.Name;
        }
    }
}

