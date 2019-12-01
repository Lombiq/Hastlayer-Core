using Hast.Layer;
using Hast.Transformer.Abstractions.Configuration;
using ICSharpCode.Decompiler.CSharp;
using System.Collections.Generic;

namespace Hast.Transformer.Services
{
    public class TaskBodyInvocationInstanceCountsSetter : ITaskBodyInvocationInstanceCountsSetter
    {
        public void SetTaskBodyInvocationInstanceCounts(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            syntaxTree.AcceptVisitor(new TaskBodyInvocationInstanceCountsSetterVisitor(configuration.TransformerConfiguration()));
        }


        private class TaskBodyInvocationInstanceCountsSetterVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, int> _taskStartsCountInMembers = new Dictionary<string, int>();
            private readonly TransformerConfiguration _configuration;


            public TaskBodyInvocationInstanceCountsSetterVisitor(TransformerConfiguration configuration)
            {
                _configuration = configuration;
            }


            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                if (!memberReferenceExpression.IsTaskStartNew()) return;

                var parentEntity = memberReferenceExpression.FindFirstParentEntityDeclaration();
                var parentEntityName = parentEntity.GetFullName();

                _taskStartsCountInMembers.TryGetValue(parentEntityName, out int taskStartsCountInMember);
                _taskStartsCountInMembers[parentEntityName] = taskStartsCountInMember + 1;

                var invokingMemberMaxInvocationConfiguration = _configuration
                    .GetMaxInvocationInstanceCountConfigurationForMember(MemberInvocationInstanceCountConfiguration
                        .AddLambdaExpressionIndexToSimpleName(parentEntity.GetSimpleName(), taskStartsCountInMember));

                // Only do something if there's no invocation instance count configured.
                if (invokingMemberMaxInvocationConfiguration.MaxInvocationInstanceCount != 1) return;

                // Searching for a parent while statement that has a condition with a variable and a primitive expression,
                // i.e. something like num < 10.

                var parentWhile = memberReferenceExpression.FindFirstParentOfType<WhileStatement>();

                if (parentWhile == null ||
                    !parentWhile.Condition.Is<BinaryOperatorExpression>(
                        expression => expression.Right is IdentifierExpression || expression.Left is IdentifierExpression,
                        out var condition))
                {
                    return;
                }

                var primitiveExpression = condition.Left as PrimitiveExpression ?? condition.Left.FindFirstChildOfType<PrimitiveExpression>();
                if (primitiveExpression == null)
                {
                    primitiveExpression = condition.Right as PrimitiveExpression ?? condition.Right.FindFirstChildOfType<PrimitiveExpression>();

                    if (condition.Right.Is<BinaryOperatorExpression>(out var innerCondition))
                    {
                        // In code decopmiled from F# it can happen that the expression will be decompiled into 
                        // "1 + actual number"... Taking care of that here.
                        primitiveExpression = innerCondition.Right as PrimitiveExpression ?? innerCondition.Right.FindFirstChildOfType<PrimitiveExpression>();
                    }
                }

                if (primitiveExpression == null) return;

                var valueString = primitiveExpression.Value.ToString();

                if (!int.TryParse(valueString, out var value)) return;

                if (condition.Operator == BinaryOperatorType.LessThan)
                {
                    invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism = value;
                }
                else if (condition.Operator == BinaryOperatorType.LessThanOrEqual)
                {
                    invokingMemberMaxInvocationConfiguration.MaxDegreeOfParallelism = value - 1;
                }
            }
        }
    }
}
