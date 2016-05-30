using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Visitors
{
    /// <summary>
    /// When a member's instance count is >1 the members invoked by it should have at least that instance count. This
    /// visitor adjusts these instance counts.
    /// </summary>
    internal class InvokationInstanceCountAdjustingVisitor : DepthFirstAstVisitor
    {
        private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
        private readonly TransformerConfiguration _transformerConfiguration;


        public InvokationInstanceCountAdjustingVisitor(
            ITypeDeclarationLookupTable typeDeclarationLookupTable,
            IHardwareGenerationConfiguration hardwareGenerationConfiguration)
        {
            _typeDeclarationLookupTable = typeDeclarationLookupTable;
            _transformerConfiguration = hardwareGenerationConfiguration.TransformerConfiguration();
        }


        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            base.VisitMemberReferenceExpression(memberReferenceExpression);

            var referencedMember = memberReferenceExpression.GetMemberDeclaration(_typeDeclarationLookupTable);

            if (referencedMember == null) return;

            var referencedMemberMaxInvokationConfiguration = _transformerConfiguration
                .GetMaxInvokationInstanceCountConfigurationForMember(referencedMember);

            var invokingMemberMaxInvokationConfiguration = _transformerConfiguration
                .GetMaxInvokationInstanceCountConfigurationForMember(memberReferenceExpression.FindFirstParentOfType<EntityDeclaration>());

            if (invokingMemberMaxInvokationConfiguration.MaxInvokationInstanceCount > referencedMemberMaxInvokationConfiguration.MaxInvokationInstanceCount)
            {
                referencedMemberMaxInvokationConfiguration.MaxDegreeOfParallelism = invokingMemberMaxInvokationConfiguration.MaxDegreeOfParallelism;
                referencedMemberMaxInvokationConfiguration.MaxRecursionDepth = invokingMemberMaxInvokationConfiguration.MaxRecursionDepth;
            }
        }
    }
}
