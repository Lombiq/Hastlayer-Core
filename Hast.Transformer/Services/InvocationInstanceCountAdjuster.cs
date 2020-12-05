using Hast.Common.Configuration;
using Hast.Layer;
using Hast.Transformer.Abstractions.Configuration;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;

namespace Hast.Transformer.Services
{
    public class InvocationInstanceCountAdjuster : IInvocationInstanceCountAdjuster
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;

        public InvocationInstanceCountAdjuster(ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
        }

        public void AdjustInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration) => syntaxTree.AcceptVisitor(new InvocationInstanceCountAdjustingVisitor(
                _typeDeclarationLookupTableFactory.Create(syntaxTree),
                configuration));

        private class InvocationInstanceCountAdjustingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
            private readonly TransformerConfiguration _transformerConfiguration;
            private readonly HashSet<EntityDeclaration> _membersInvokedFromNonParallel = new HashSet<EntityDeclaration>();
            private readonly HashSet<EntityDeclaration> _membersInvokedFromNonRecursive = new HashSet<EntityDeclaration>();

            public InvocationInstanceCountAdjustingVisitor(
                ITypeDeclarationLookupTable typeDeclarationLookupTable,
                IHardwareGenerationConfiguration hardwareGenerationConfiguration)
            {
                _typeDeclarationLookupTable = typeDeclarationLookupTable;
                _transformerConfiguration = hardwareGenerationConfiguration.TransformerConfiguration();
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                if (memberReferenceExpression.FindFirstParentOfType<ICSharpCode.Decompiler.CSharp.Syntax.Attribute>() != null)
                {
                    return;
                }

                AdjustInstanceCount(
                    memberReferenceExpression,
                    memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable));
            }

            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                // Funcs are only needed for Task-based parallelism, omitting them for now.
                if (objectCreateExpression.Type.GetActualType().IsFunc())
                {
                    return;
                }

                AdjustInstanceCount(
                    objectCreateExpression,
                    objectCreateExpression.FindConstructorDeclaration(_typeDeclarationLookupTable));
            }

            private void AdjustInstanceCount(Expression referencingExpression, EntityDeclaration referencedMember)
            {
                if (referencedMember == null) return;

                var referencedMemberMaxInvocationConfiguration = _transformerConfiguration
                    .GetMaxInvocationInstanceCountConfigurationForMember(referencedMember);

                var invokingMemberMaxInvocationConfiguration = _transformerConfiguration
                    .GetMaxInvocationInstanceCountConfigurationForMember(referencingExpression.FindFirstParentOfType<EntityDeclaration>());

                var referencedMemberFullName = referencedMember.GetFullName();

                if (invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism > referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism)
                {
                    referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism = invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism;

                    // Was the referenced member invoked both from a parallelized member and from a non-parallelized
                    // one? Then let's increase MaxDegreeOfParallelism so there is no large multiplexing logic needed,
                    // instances of the referenced and invoking members can be paired.
                    if (_membersInvokedFromNonParallel.Contains(referencedMember))
                    {
                        referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism++;
                    }
                }
                else if (invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism == 1 &&
                    !(referencedMemberFullName.IsDisplayOrClosureClassMemberName() || referencedMemberFullName.IsInlineCompilerGeneratedMethodName()))
                {
                    _membersInvokedFromNonParallel.Add(referencedMember);

                    // Same increment as above. Just needed so it doesn't matter whether the parallelized or the
                    // non-parallelized invoking member is processed first.
                    if (referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism != 1)
                    {
                        referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism++;
                    }
                }

                if (invokingMemberMaxInvocationConfiguration.MaxRecursionDepth > referencedMemberMaxInvocationConfiguration.MaxRecursionDepth)
                {
                    referencedMemberMaxInvocationConfiguration.MaxRecursionDepth = invokingMemberMaxInvocationConfiguration.MaxRecursionDepth;

                    // Same logic here than above with MaxDegreeOfParallelism.
                    if (_membersInvokedFromNonRecursive.Contains(referencedMember))
                    {
                        referencedMemberMaxInvocationConfiguration.MaxRecursionDepth++;
                    }
                }
                else if (invokingMemberMaxInvocationConfiguration.MaxRecursionDepth == 1 &&
                    !(referencedMemberFullName.IsDisplayOrClosureClassMemberName() || referencedMemberFullName.IsInlineCompilerGeneratedMethodName()))
                {
                    _membersInvokedFromNonParallel.Add(referencedMember);

                    if (referencedMemberMaxInvocationConfiguration.MaxRecursionDepth != 1)
                    {
                        referencedMemberMaxInvocationConfiguration.MaxRecursionDepth++;
                    }
                }
            }
        }
    }
}
