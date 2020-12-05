using Hast.Common.Helpers;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class ConditionalExpressionsToIfElsesConverter : IConditionalExpressionsToIfElsesConverter
    {
        public void ConvertConditionalExpressionsToIfElses(SyntaxTree syntaxTree) => syntaxTree.AcceptVisitor(new ConditionalExpressionsConvertingVisitor());

        private class ConditionalExpressionsConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitConditionalExpression(ConditionalExpression conditionalExpression)
            {
                base.VisitConditionalExpression(conditionalExpression);

                var assignment = conditionalExpression.Parent as AssignmentExpression;

                // Simple "variable = condition ? value1 : value2" expressions are easily handled but ones not in this
                // form need some further work.
                if (assignment == null ||
                    !(assignment.Left is IdentifierExpression) ||
                    !(assignment.Parent is ExpressionStatement))
                {
                    var variableName = "conditional" + Sha2456Helper.ComputeHash(conditionalExpression.GetFullName());

                    var resolveResult = conditionalExpression.GetResolveResult();

                    var variableType = resolveResult.Type;

                    ILVariableResolveResult CreateILVariableResolveResult() =>
                        VariableHelper.CreateILVariableResolveResult(VariableKind.Local, variableType, variableName);

                    // First creating a variable for the result.
                    var variableDeclaration =
                        new VariableDeclarationStatement(TypeHelper.CreateAstType(variableType), variableName);
                    variableDeclaration.Variables.Single().AddAnnotation(CreateILVariableResolveResult());
                    AstInsertionHelper.InsertStatementBefore(
                        conditionalExpression.FindFirstParentStatement(),
                        variableDeclaration);

                    // Then moving the conditional expression so its result is assigned to the variable.
                    var newConditionalExpression = conditionalExpression.Clone<ConditionalExpression>();
                    assignment = new AssignmentExpression(
                        new IdentifierExpression(variableName).WithAnnotation(CreateILVariableResolveResult()),
                        newConditionalExpression);

                    assignment.AddAnnotation(resolveResult);

                    AstInsertionHelper.InsertStatementAfter(variableDeclaration, new ExpressionStatement(assignment));

                    // And finally swapping out the original expression with the variable reference.
                    conditionalExpression.ReplaceWith(new IdentifierExpression(variableName).WithAnnotation(CreateILVariableResolveResult()));
                    conditionalExpression = newConditionalExpression;
                }

                // Enclosing the assignments into BlockStatements because this is also what normal if-else statements
                // are decompiled into. This is also necessary to establish a variable scope.
                var trueAssignment = assignment.Clone<AssignmentExpression>();
                trueAssignment.Right = conditionalExpression.TrueExpression.Clone();
                var trueBlock = new BlockStatement();
                trueBlock.Statements.Add(new ExpressionStatement(trueAssignment));

                var falseAssignment = assignment.Clone<AssignmentExpression>();
                falseAssignment.Right = conditionalExpression.FalseExpression.Clone();
                var falseBlock = new BlockStatement();
                falseBlock.Statements.Add(new ExpressionStatement(falseAssignment));

                conditionalExpression.Parent.Parent
                    .ReplaceWith(new IfElseStatement(
                        conditionalExpression.Condition.Clone(), trueBlock, falseBlock));
            }
        }
    }
}
