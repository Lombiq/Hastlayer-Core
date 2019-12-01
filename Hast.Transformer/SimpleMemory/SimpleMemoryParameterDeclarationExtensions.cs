
using Mono.Cecil;

namespace ICSharpCode.Decompiler.CSharp
{
    public static class SimpleMemoryParameterDeclarationExtensions
    {
        public static bool IsSimpleMemoryParameter(this ParameterDeclaration parameterDeclaration) =>
            parameterDeclaration.Type.GetActualTypeReference().IsSimpleMemory();
    }
}
