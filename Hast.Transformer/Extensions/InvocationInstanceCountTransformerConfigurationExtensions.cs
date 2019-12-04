﻿using Hast.Transformer.Abstractions.Configuration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Common.Configuration
{
    public static class InvocationInstanceCountTransformerConfigurationExtensions
    {
        public static MemberInvocationInstanceCountConfiguration GetMaxInvocationInstanceCountConfigurationForMember(
            this TransformerConfiguration configuration,
            EntityDeclaration entity)
        {
            var fullName = entity.GetFullName();
            var simpleName = entity.GetSimpleName();
            var isDisplayClassMember = fullName.IsDisplayOrClosureClassMemberName();
            var isIsInlineCompilerGeneratedMethod = fullName.IsInlineCompilerGeneratedMethodName();

            if (!isDisplayClassMember && !isIsInlineCompilerGeneratedMethod)
            {
                return configuration.GetMaxInvocationInstanceCountConfigurationForMember(simpleName);
            }

            // If this is a DisplayClass member then it was generated from a lambda expression. So need to handle it
            // with the special "MemberNamePrefix.LambdaExpression.[Index]" pattern.

            var indexedNameHolder = entity.Annotation<LambdaExpressionIndexedNameHolder>();

            // If there is no IndexedNameHolder then first we need to generate the indices for all lambdas.
            if (indexedNameHolder == null)
            {
                TypeDeclaration parentType;
                IEnumerable<EntityDeclaration> compilerGeneratedMembers;

                if (isDisplayClassMember)
                {
                    // Run the index-setting logic on the members of the parent class.

                    parentType = entity
                        .FindFirstParentTypeDeclaration() // The DisplayClass.
                        .FindFirstParentTypeDeclaration(); // The parent type.

                    compilerGeneratedMembers = parentType.Members
                        .Where(member => member.GetFullName().IsDisplayOrClosureClassName())
                        .SelectMany(displayClass => ((TypeDeclaration)displayClass).Members);
                }
                else
                {
                    parentType = entity.FindFirstParentTypeDeclaration();

                    compilerGeneratedMembers = parentType.Members
                        .Where(member => member.GetFullName().IsInlineCompilerGeneratedMethodName());
                }

                var compilerGeneratedMembersDictionary = compilerGeneratedMembers
                    .ToDictionary(member => member.GetFullName());
                parentType.AcceptVisitor(new IndexedNameHolderSettingVisitor(compilerGeneratedMembersDictionary));

                indexedNameHolder = entity.Annotation<LambdaExpressionIndexedNameHolder>();

                // If it's still null then the member wasn't generated from a lambda expression and thus normal rules
                // apply.
                if (indexedNameHolder == null)
                {
                    return configuration.GetMaxInvocationInstanceCountConfigurationForMember(simpleName);
                }
            }

            return configuration.GetMaxInvocationInstanceCountConfigurationForMember(indexedNameHolder.IndexedName);
        }


        private class LambdaExpressionIndexedNameHolder
        {
            public string IndexedName { get; set; }
        }

        private class IndexedNameHolderSettingVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, EntityDeclaration> _compilerGeneratedMembers;
            private readonly Dictionary<EntityDeclaration, int> _lambdaCounts = new Dictionary<EntityDeclaration, int>();


            public IndexedNameHolderSettingVisitor(Dictionary<string, EntityDeclaration> compilerGeneratedMembers)
            {
                _compilerGeneratedMembers = compilerGeneratedMembers;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                // Only dealing with method references.
                if (!memberReferenceExpression.IsMethodReference()) return;

                var memberFullName = memberReferenceExpression.GetMemberFullName();

                if (!memberFullName.IsDisplayOrClosureClassMemberName() && !memberFullName.IsInlineCompilerGeneratedMethodName())
                {
                    return;
                }

                if (_compilerGeneratedMembers.TryGetValue(memberFullName, out EntityDeclaration member))
                {
                    if (member.Annotation<LambdaExpressionIndexedNameHolder>() == null)
                    {
                        var parentMember = memberReferenceExpression.FindFirstParentOfType<EntityDeclaration>();

                        if (!_lambdaCounts.ContainsKey(parentMember))
                        {
                            _lambdaCounts[parentMember] = 0;
                        }

                        member.AddAnnotation(new LambdaExpressionIndexedNameHolder
                        {
                            IndexedName = MemberInvocationInstanceCountConfiguration
                                .AddLambdaExpressionIndexToSimpleName(parentMember.GetSimpleName(), _lambdaCounts[parentMember])
                        });

                        _lambdaCounts[parentMember]++;
                    }
                }
            }
        }
    }
}
