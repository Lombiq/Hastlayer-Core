using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class SimpleMemoryMethodDeclarationExtensions
    {
        public static string GetSimpleMemoryParameterName(this MethodDeclaration methodDeclaration)
        {
            var parameter = methodDeclaration.Parameters.SingleOrDefault(p => p.IsSimpleMemoryParameter());
            if (parameter == null) return null;
            return parameter.Name;
        }
    }
}

