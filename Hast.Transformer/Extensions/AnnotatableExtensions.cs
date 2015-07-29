using System;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AnnotatableExtensions
    {
        public static TypeReference GetActualType(this IAnnotatable annotable)
        {
            var typeInformation = annotable.Annotation<TypeInformation>();
            if (typeInformation != null) return typeInformation.GetActualType();

            var ilVariable = annotable.Annotation<ILVariable>();
            if (ilVariable != null) return ilVariable.Type;

            return null;
        }
    }
}
