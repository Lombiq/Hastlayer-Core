using Hast.Common.Configuration;
using Hast.Layer;
using Hast.Transformer.Abstractions.Configuration;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services
{
    public class InvocationInstanceCountAdjuster : IInvocationInstanceCountAdjuster
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;


        public InvocationInstanceCountAdjuster(ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
        }


        public void AdjustInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            syntaxTree.AcceptVisitor(new InvocationInstanceCountAdjustingVisitor(
                _typeDeclarationLookupTableFactory.Create(syntaxTree),
                configuration));
        }


        private class InvocationInstanceCountAdjustingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
            private readonly TransformerConfiguration _transformerConfiguration;


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

                AdjustInstanceCount(
                    memberReferenceExpression, 
                    memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable));
            }

            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                // Funcs are only needed for Task-based parallelism, omitting them for now.
                if (objectCreateExpression.Type.GetFullName().StartsWith("System.Func"))
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

                if (invokingMemberMaxInvocationConfiguration.MaxInvocationInstanceCount > referencedMemberMaxInvocationConfiguration.MaxInvocationInstanceCount)
                {
                    referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism = invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism;
                    referencedMemberMaxInvocationConfiguration.MaxRecursionDepth = invokingMemberMaxInvocationConfiguration.MaxRecursionDepth;
                }
            }
        }
    }
}