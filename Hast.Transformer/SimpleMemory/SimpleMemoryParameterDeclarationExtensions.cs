using ICSharpCode.Decompiler.CSharp.Syntax;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class SimpleMemoryParameterDeclarationExtensions
    {
        public static bool IsSimpleMemoryParameter(this ParameterDeclaration parameterDeclaration) =>
            parameterDeclaration.Type.GetActualTypeReference().IsSimpleMemory();
    }
}
