using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.CSharp;

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

                var typeReference = assignmentExpression.GetActualTypeReference();

                if (assignmentExpression.Parent is Statement ||
                    assignmentExpression.Parent is ICSharpCode.Decompiler.CSharp.Attribute ||
                    // This is a DisplayClass-related if, those are handled specially later on.
                    typeReference.FullName.StartsWith("System.Func`2<System.Object,"))
                {
                    return;
                }

                // Saving the right side of the assignment to a variable and then using that instead of the original
                // embedded assignment. Not using the left side directly later because that can be any complex value
                // access, keeping it simple.
                var variableIdentifier = VariableHelper.DeclareAndReferenceVariable(
                    "assignment", 
                    assignmentExpression, 
                    AstBuilder.ConvertType(typeReference));

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
