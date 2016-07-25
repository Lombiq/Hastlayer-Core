using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Visitors
{
    internal class InlinableTaskArraysInliningVisitor : DepthFirstAstVisitor
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
                    parentAssignment.Right.GetActualTypeReference().FullName.StartsWith("System.Threading.Tasks.Task`1<"))
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
