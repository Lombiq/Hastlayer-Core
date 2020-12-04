using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class GeneratedTaskArraysInliner : IGeneratedTaskArraysInliner
    {
        public void InlineGeneratedTaskArrays(SyntaxTree syntaxTree)
        {
            var inlinableTaskArraysFindingVisitor = new InlinableTaskArraysFindingVisitor();
            syntaxTree.AcceptVisitor(inlinableTaskArraysFindingVisitor);
            syntaxTree.AcceptVisitor(new InlinableTaskArraysInliningVisitor(inlinableTaskArraysFindingVisitor.InlinableVariableMapping));
        }

        private class InlinableTaskArraysFindingVisitor : DepthFirstAstVisitor
        {
            public Dictionary<string, string> InlinableVariableMapping { get; set; } = new Dictionary<string, string>();

            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                // AssigmentExpression, Left = arg_*, Right.GetActualTypeFullName().StartsWith("System.Threading.Tasks.Task`1<")

                var compilerGeneratedVariableName = string.Empty;
                if (assignmentExpression.Left.Is<IdentifierExpression>(identifier =>
                        (compilerGeneratedVariableName = identifier.Identifier).StartsWith("arg_")) &&
                    assignmentExpression.Right.GetActualTypeFullName().StartsWith("System.Threading.Tasks.Task`1<"))
                {
                    InlinableVariableMapping[compilerGeneratedVariableName] =
                        ((IdentifierExpression)assignmentExpression.Right).Identifier;
                }
            }
        }

        private class InlinableTaskArraysInliningVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, string> _inlinableVariableMappings;

            public InlinableTaskArraysInliningVisitor(Dictionary<string, string> inlinableVariableMappings)
            {
                _inlinableVariableMappings = inlinableVariableMappings;
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                if (IsInlinableVariableIdentifier(identifierExpression))
                {
                    var parentAssignment = identifierExpression.FindFirstParentOfType<AssignmentExpression>();
                    // If this is in an arg_9C_0 = array; kind of assignment, then remove the whole assignment's statement.
                    if (parentAssignment != null &&
                        parentAssignment.Left == identifierExpression &&
                        parentAssignment.Right.GetActualTypeFullName().StartsWith("System.Threading.Tasks.Task`1<"))
                    {
                        parentAssignment.FindFirstParentOfType<ExpressionStatement>().Remove();
                    }
                    else
                    {
                        identifierExpression.Identifier = _inlinableVariableMappings[identifierExpression.Identifier];
                    }
                }
            }

            public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
            {
                base.VisitVariableDeclarationStatement(variableDeclarationStatement);

                if (_inlinableVariableMappings.ContainsKey(variableDeclarationStatement.Variables.Single().Name))
                {
                    variableDeclarationStatement.Remove();
                }
            }

            private bool IsInlinableVariableIdentifier(IdentifierExpression identifierExpression)
            {
                return _inlinableVariableMappings.ContainsKey(identifierExpression.Identifier);
            }
        }
    }
}
