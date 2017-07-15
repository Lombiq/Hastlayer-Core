using Hast.Transformer.Abstractions.SimpleMemory;
using ICSharpCode.Decompiler.Ast;

namespace Mono.Cecil
{
    public static class TypeReferenceExtensions
    {
        public static bool IsSimpleMemory(this TypeReference typeReference) =>
            typeReference != null && typeReference.FullName == typeof(SimpleMemory).FullName;

        public static TypeInformation ToTypeInformation(this TypeReference typeReference) =>
            new TypeInformation(typeReference, typeReference);
    }
}
