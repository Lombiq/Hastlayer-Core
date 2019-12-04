﻿using Hast.Transformer.Models;
using System;
using System.Linq;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class MemberReferenceExpressionExtensions
    {
        /// <summary>
        /// Find the referenced member's declaration.
        /// </summary>
        /// <param name="typeDeclarationLookupTable">
        /// The <see cref="ITypeDeclarationLookupTable"/> instance corresponding to the current scope.
        /// </param>
        /// <param name="findLeftmostMemberIfRecursive">
        /// If the member reference references another member (like <c>this.Property1.Property2.Property3</c>) then if 
        /// set to <c>true</c> the member corresponding to the leftmost member (<c>this.Property1</c> in this case) will
        /// be looked up.
        /// </param>
        public static EntityDeclaration FindMemberDeclaration(
            this MemberReferenceExpression memberReferenceExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable,
            bool findLeftmostMemberIfRecursive = false)
        {
            TypeDeclaration type;

            if (memberReferenceExpression.Target is MemberReferenceExpression)
            {
                if (findLeftmostMemberIfRecursive)
                {
                    return ((MemberReferenceExpression)memberReferenceExpression.Target).FindMemberDeclaration(typeDeclarationLookupTable, true);
                }
                else
                {
                    type = typeDeclarationLookupTable.Lookup(memberReferenceExpression.Target.GetActualTypeReference().FullName);
                }
            }
            else
            {
                type = memberReferenceExpression.FindTargetTypeDeclaration(typeDeclarationLookupTable);
            }

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
                // A FieldReference annotation is present if the expression is about accessing a field.
                var fieldReference = memberReferenceExpression.Annotation<FieldReference>();
                if (fieldReference != null)
                {
                    if (!fieldReference.FullName.IsBackingFieldName())
                    {
                        return type.Members
                            .SingleOrDefault(member => member.Annotation<IMemberDefinition>().FullName == fieldReference.FullName);
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

        public static TypeDeclaration FindTargetTypeDeclaration(
            this MemberReferenceExpression memberReferenceExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var target = memberReferenceExpression.Target;

            if (target is TypeReferenceExpression)
            {
                // The member is in a different class.
                return typeDeclarationLookupTable.Lookup((TypeReferenceExpression)target);
            }
            else if (target is BaseReferenceExpression)
            {
                // The member is in the base class (because of single class inheritance in C#, there can be only one base class).
                return memberReferenceExpression.FindFirstParentTypeDeclaration().BaseTypes
                    .Select(type => typeDeclarationLookupTable.Lookup(type))
                    .SingleOrDefault(typeDeclaration => typeDeclaration != null && typeDeclaration.ClassType == ClassType.Class);
            }
            else if (target is IdentifierExpression || target is IndexerExpression)
            {
                var typeReference = target.GetActualTypeReference();
                return typeReference == null ? null : typeDeclarationLookupTable.Lookup(typeReference.FullName);
            }
            else if (target is MemberReferenceExpression)
            {
                return ((MemberReferenceExpression)target).FindTargetTypeDeclaration(typeDeclarationLookupTable);
            }
            else if (target is ObjectCreateExpression)
            {
                // The member is referenced in an object initializer.
                return typeDeclarationLookupTable.Lookup(((ObjectCreateExpression)target).Type);
            }
            else if (target is InvocationExpression)
            {
                var methodReference = memberReferenceExpression.Annotation<MethodReference>();
                if (methodReference != null)
                {
                    return typeDeclarationLookupTable.Lookup(methodReference.DeclaringType.FullName);
                }
            }

            // The member is within this class.
            return memberReferenceExpression.FindFirstParentTypeDeclaration();
        }

        public static string GetMemberFullName(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.GetReferencedMemberFullName();

        /// <summary>
        /// Determines if the member reference is an access to an array's Length property.
        /// </summary>
        public static bool IsArrayLengthAccess(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.MemberName == "Length" && memberReferenceExpression.Target.GetActualTypeReference().IsArray;

        public static bool IsTaskStartNew(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.MemberName == "StartNew" &&
            memberReferenceExpression.Target.GetActualTypeReference().FullName == typeof(System.Threading.Tasks.TaskFactory).FullName;

        public static bool IsMethodReference(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.Annotation<MethodDefinition>() != null;

        public static bool IsFieldReference(this MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.Annotation<FieldDefinition>() != null;
    }
}
