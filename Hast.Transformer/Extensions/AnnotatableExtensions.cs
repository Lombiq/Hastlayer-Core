using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AnnotatableExtensions
    {
        public static TypeReference GetActualType(this IAnnotatable annotable)
        {
            return annotable.Annotation<TypeInformation>().GetActualType();
        }
    }
}
