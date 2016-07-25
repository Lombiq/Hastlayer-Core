using System;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AnnotatableExtensions
    {
        public static TypeReference GetActualTypeReference(this IAnnotatable annotable, bool getExpectedType = false)
        {
            var typeInformation = annotable.Annotation<TypeInformation>();
            if (typeInformation != null)
            {
                if (getExpectedType) return typeInformation.ExpectedType;
                else return typeInformation.InferredType ?? typeInformation.ExpectedType;
            }

            var ilVariable = annotable.Annotation<ILVariable>();
            if (ilVariable != null) return ilVariable.Type;

            return null;
        }
    }
}
