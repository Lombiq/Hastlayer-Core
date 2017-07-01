using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.NRefactory.CSharp;


namespace Hast.Transformer.Services
{
    public class ConditionalExpressionsToIfElsesConverter : IConditionalExpressionsToIfElsesConverter
    {
        public void ConvertConditionalExpressionsToIfElses(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new ConditionalExpressionsConvertingVisitor());
        }


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
                    var variableName = "conditional" + Sha2456Helper.ComputeHash(conditionalExpression.ToString());

                    var typeInformation = conditionalExpression.Annotation<TypeInformation>();
                    if (typeInformation == null)
                    {
                        // If a conditional expression is inside a cast then a TypeInformation annotation will be only
                        // on the cast.
                        typeInformation = conditionalExpression
                            .FindFirstParentOfType<CastExpression>()
                            .Annotation<TypeInformation>();
                    }

                    var variableTypeReference = typeInformation.InferredType;

                    // First creating a variable for the result.
                    var variableDeclaration = 
                        new VariableDeclarationStatement(AstBuilder.ConvertType(variableTypeReference), variableName);
                    AstInsertionHelper.InsertStatementBefore(
                        conditionalExpression.FindFirstParentStatement(),
                        variableDeclaration);

                    // Then moving the conditational expression so its result is assigned to the variable.
                    var variableIdentifier = new IdentifierExpression(variableName);
                    variableIdentifier.AddAnnotation(new ILVariable { Name = variableName, Type = variableTypeReference });
                    var newConditionalExpression = (ConditionalExpression)conditionalExpression.Clone();
                    assignment = new AssignmentExpression(variableIdentifier, newConditionalExpression);


                    assignment.AddAnnotation(typeInformation);

                    AstInsertionHelper.InsertStatementAfter(variableDeclaration, new ExpressionStatement(assignment));

                    // And finally swapping out the original expression with the variable reference.
                    conditionalExpression.ReplaceWith(variableIdentifier.Clone());
                    conditionalExpression = newConditionalExpression;
                }

                // Enclosing the assignments into BlockStatements because this is also what normal if-else statements 
                // are decompiled into. This is also necessary to establish a variable scope.
                var trueAssignment = (AssignmentExpression)assignment.Clone();
                trueAssignment.Right = conditionalExpression.TrueExpression.Clone();
                var trueBlock = new BlockStatement();
                trueBlock.Statements.Add(new ExpressionStatement(trueAssignment));

                var falseAssignment = (AssignmentExpression)assignment.Clone();
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
