﻿using ICSharpCode.Decompiler.Ast;
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

            var typeReference = annotable.Annotation<TypeReference>();
            if (typeReference != null) return typeReference;

            var ilVariable = annotable.Annotation<ILVariable>();
            if (ilVariable != null) return ilVariable.Type;

            var fieldReference = annotable.Annotation<FieldReference>();
            if (fieldReference != null) return fieldReference.FieldType;

            var propertyReference = annotable.Annotation<PropertyReference>();
            if (propertyReference != null) return propertyReference.PropertyType;

            return null;
        }

        public static void CopyAnnotationsTo(this IAnnotatable annotable, IAnnotatable toNode)
        {
            foreach (var annotation in annotable.Annotations)
            {
                toNode.AddAnnotation(annotation);
            }
        }
    }
}
