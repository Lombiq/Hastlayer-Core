using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    public class EmbeddedAssignmentExpressionsExpander : IEmbeddedAssignmentExpressionsExpander
    {
        public void ExpandEmbeddedAssignmentExpressions(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new EmbeddedAssignmentExpressionsExpandingVisitor());
        }


        private class EmbeddedAssignmentExpressionsExpandingVisitor : DepthFirstAstVisitor
        {
            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                var type = assignmentExpression.GetActualType();

                if (assignmentExpression.Parent is Statement ||
                    assignmentExpression.Parent is Attribute ||
                    // This is a DisplayClass-related if, those are handled specially later on.
                    type.FullName.StartsWith("System.Func`2<System.Object,"))
                {
                    return;
                }

                // Saving the right side of the assignment to a variable and then using that instead of the original
                // embedded assignment. Not using the left side directly later because that can be any complex value
                // access, keeping it simple.
                var variableIdentifier = VariableHelper.DeclareAndReferenceVariable(
                    "assignment",
                    assignmentExpression,
                    AstBuildingHelper.ConvertType(type));

                var firstParentStatement = assignmentExpression.FindFirstParentStatement();
                var typeInformation = assignmentExpression.GetTypeInformationOrCreateFromActualTypeReference();

                var tempVariableAssignment = new AssignmentExpression(variableIdentifier, assignmentExpression.Right.Clone())
                    .WithAnnotation(typeInformation);

                AstInsertionHelper.InsertStatementBefore(firstParentStatement, new ExpressionStatement(tempVariableAssignment));

                var leftAssignment = new AssignmentExpression(assignmentExpression.Left.Clone(), variableIdentifier.Clone())
                    .WithAnnotation(typeInformation);

                AstInsertionHelper.InsertStatementBefore(firstParentStatement, new ExpressionStatement(leftAssignment));


                assignmentExpression.ReplaceWith(variableIdentifier.Clone());
            }
        }
    }
}
