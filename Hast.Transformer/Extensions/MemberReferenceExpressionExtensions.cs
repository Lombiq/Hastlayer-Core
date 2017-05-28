﻿using System;
using System.Linq;
using Hast.Transformer.Models;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class MemberReferenceExpressionExtensions
    {
        public static EntityDeclaration GetMemberDeclaration(
            this MemberReferenceExpression memberReferenceExpression, 
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var type = memberReferenceExpression.GetTargetTypeDeclaration(typeDeclarationLookupTable);

            if (type == null) return null;

            // A MethodReference annotation is present if the expression is for creating a delegate for a lambda expression
            // like this: new Func<object, bool> (<>c__DisplayClass.<ParallelizedArePrimeNumbersAsync>b__0)
            var methodReference = memberReferenceExpression.Annotation<MethodReference>();
            if (methodReference != null)
            {
                var memberName = methodReference.FullName;

                // If this is a member reference to a property then both a MethodReference (for the setter or getter)
                // and a PropertyReference will be there, but the latter will contain the member name.
                var propertyReference = memberReferenceExpression.Annotation<PropertyReference>();
                if (propertyReference != null) memberName = propertyReference.FullName;

                return type.Members
                    .SingleOrDefault(member => member.Annotation<IMemberDefinition>().FullName == memberName);
            }
            else
            {
                // A FieldDefinition annotation is present if the expression is about accessing a field.
                var fieldDefinition = memberReferenceExpression.Annotation<FieldDefinition>();
                if (fieldDefinition != null)
                {
                    if (!fieldDefinition.FullName.IsBackingFieldName())
                    {
                        return type.Members
                            .SingleOrDefault(member => member.Annotation<IMemberDefinition>().FullName == fieldDefinition.FullName); 
                    }
                    else
                    {
                        return type.Members
                            .SingleOrDefault(member => member.Name == memberReferenceExpression.MemberName && member is PropertyDeclaration);
                    }
                }
                else
                {
                    var parent = memberReferenceExpression.Parent;
                    MemberReference memberReference = null;
                    while (memberReference == null && parent != null)
                    {
                        memberReference = parent.Annotation<MemberReference>();
                        parent = parent.Parent;
                    }

                    var declaringType = typeDeclarationLookupTable.Lookup(memberReference.DeclaringType.FullName);

                    if (declaringType == null) return null;

                    return
                        declaringType
                        .Members
                        .SingleOrDefault(member => member.Annotation<MemberReference>().FullName == memberReference.FullName);
                }
            }
        }

        public static TypeDeclaration GetTargetTypeDeclaration(
            this MemberReferenceExpression memberReferenceExpression, 
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            if (memberReferenceExpression.Target is TypeReferenceExpression)
            {
                // The member is in a different class.
                return typeDeclarationLookupTable.Lookup((TypeReferenceExpression)memberReferenceExpression.Target);
            }
            else if (memberReferenceExpression.Target is BaseReferenceExpression)
            {
                // The member is in the base class (because of single class inheritance in C#, there can be only one base class).
                return memberReferenceExpression.FindFirstParentTypeDeclaration().BaseTypes
                    .Select(type => typeDeclarationLookupTable.Lookup(type))
                    .SingleOrDefault(typeDeclaration => typeDeclaration != null && typeDeclaration.ClassType == ClassType.Class);
            }
            else if (memberReferenceExpression.Target is IdentifierExpression)
            {
                return typeDeclarationLookupTable.Lookup(memberReferenceExpression.Target.GetActualTypeReference().FullName);
            }
            else if (memberReferenceExpression.Target is MemberReferenceExpression)
            {
                return ((MemberReferenceExpression)memberReferenceExpression.Target).GetTargetTypeDeclaration(typeDeclarationLookupTable);
            }
            else
            {
                // The member is within this class.
                return memberReferenceExpression.FindFirstParentTypeDeclaration();
            }
        }
    }
}
