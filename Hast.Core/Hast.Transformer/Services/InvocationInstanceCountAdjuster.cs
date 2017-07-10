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


        /// <summary>
        /// When a member's instance count is >1 the members invoked by it should have at least that instance count. This
        /// visitor adjusts these instance counts.
        /// </summary>
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

                var referencedMember = memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable);

                if (referencedMember == null) return;

                var referencedMemberMaxInvocationConfiguration = _transformerConfiguration
                    .GetMaxInvocationInstanceCountConfigurationForMember(referencedMember);

                var invokingMemberMaxInvocationConfiguration = _transformerConfiguration
                    .GetMaxInvocationInstanceCountConfigurationForMember(memberReferenceExpression
                    .FindFirstParentOfType<EntityDeclaration>());

                if (invokingMemberMaxInvocationConfiguration.MaxInvocationInstanceCount > referencedMemberMaxInvocationConfiguration.MaxInvocationInstanceCount)
                {
                    referencedMemberMaxInvocationConfiguration.MaxDegreeOfParallelism = invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism;
                    referencedMemberMaxInvocationConfiguration.MaxRecursionDepth = invokingMemberMaxInvocationConfiguration.MaxRecursionDepth;
                }
            }
        }
    }
}