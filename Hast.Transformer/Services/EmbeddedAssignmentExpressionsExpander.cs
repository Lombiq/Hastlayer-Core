using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;

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
                    type.IsFunc())
                {
                    return;
                }

                // Saving the right side of the assignment to a variable and then using that instead of the original
                // embedded assignment. Not using the left side directly later because that can be any complex value
                // access, keeping it simple.
                var variableIdentifier = VariableHelper.DeclareAndReferenceVariable(
                    "assignment",
                    assignmentExpression,
                    TypeHelper.CreateAstType(type));

                var firstParentStatement = assignmentExpression.FindFirstParentStatement();
                var resolveResult = assignmentExpression.CreateResolveResultFromActualType();

                var tempVariableAssignment = new AssignmentExpression(variableIdentifier, assignmentExpression.Right.Clone())
                    .WithAnnotation(resolveResult);

                AstInsertionHelper.InsertStatementBefore(firstParentStatement, new ExpressionStatement(tempVariableAssignment));

                var leftAssignment = new AssignmentExpression(assignmentExpression.Left.Clone(), variableIdentifier.Clone())
                    .WithAnnotation(resolveResult);

                AstInsertionHelper.InsertStatementBefore(firstParentStatement, new ExpressionStatement(leftAssignment));


                assignmentExpression.ReplaceWith(variableIdentifier.Clone());
            }
        }
    }
}
