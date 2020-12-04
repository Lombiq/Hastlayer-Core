using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class UnneededReferenceVariablesRemover : IUnneededReferenceVariablesRemover
    {
        public void RemoveUnneededVariables(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new AssignmentsDiscoveringVisitor());
        }

        private class AssignmentsDiscoveringVisitor : DepthFirstAstVisitor
        {
            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                var left = assignmentExpression.Left;
                var right = assignmentExpression.Right;

                var leftType = left.GetActualType();

                // Let's check whether the assignment is for a reference type and whether it's between two variables or
                // a variable and a field/property/array item access (properties at these stage are only
                // auto-properties, custom properties are already converted to methods).
                if (assignmentExpression.IsPotentialAliasAssignment())
                {
                    var parentEntity = assignmentExpression.FindFirstParentEntityDeclaration();
                    var leftIdentifierExpression = (IdentifierExpression)left;

                    // Now let's check if the left side is only ever assigned to once.
                    var assignmentsCheckingVisitor = new AssignmentsCheckingVisitor(leftIdentifierExpression.Identifier);
                    parentEntity.AcceptVisitor(assignmentsCheckingVisitor);

                    if (assignmentsCheckingVisitor.AssignedToOnce == true)
                    {
                        parentEntity.AcceptVisitor(new IdentifiersChangingVisitor(
                            leftIdentifierExpression.Identifier,
                            right));

                        parentEntity
                            .FindFirstChildOfType<VariableDeclarationStatement>(variableDeclaration =>
                                variableDeclaration.Variables.SingleOrDefault()?.Name == leftIdentifierExpression.Identifier)
                            ?.Remove();
                        assignmentExpression.FindFirstParentStatement().Remove();
                    }
                }
            }
        }

        private class AssignmentsCheckingVisitor : DepthFirstAstVisitor
        {
            private readonly string _identifier;

            public bool? AssignedToOnce { get; private set; }

            public AssignmentsCheckingVisitor(string identifier)
            {
                _identifier = identifier;
            }

            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                if (assignmentExpression.Left is IdentifierExpression identifierExpression &&
                    identifierExpression.Identifier == _identifier)
                {
                    AssignedToOnce = AssignedToOnce == null ? true : false;
                }
            }
        }

        private class IdentifiersChangingVisitor : DepthFirstAstVisitor
        {
            private readonly string _oldIdentifier;
            private readonly Expression _newExpression;

            public IdentifiersChangingVisitor(string oldIdentifier, Expression newExpression)
            {
                _oldIdentifier = oldIdentifier;
                _newExpression = newExpression;
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                if (identifierExpression.Identifier != _oldIdentifier) return;

                identifierExpression.ReplaceWith(_newExpression.Clone());
            }
        }
    }
}
