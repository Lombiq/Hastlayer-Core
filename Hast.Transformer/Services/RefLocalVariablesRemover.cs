using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;

namespace Hast.Transformer.Services
{
    public class RefLocalVariablesRemover : IRefLocalVariablesRemover
    {
        public void RemoveRefLocalVariables(SyntaxTree syntaxTree) => syntaxTree.AcceptVisitor(new InitializersChangingVisitor());

        private class InitializersChangingVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, Expression> _substitutes = new();

            public override void VisitVariableDeclarationStatement(VariableDeclarationStatement variableDeclarationStatement)
            {
                base.VisitVariableDeclarationStatement(variableDeclarationStatement);

                if (!(variableDeclarationStatement.Type is ComposedType composedType && composedType.HasRefSpecifier))
                {
                    return;
                }

                foreach (var variableInitializer in variableDeclarationStatement.Variables)
                {
                    _substitutes[variableInitializer.GetFullName()] =
                        ((DirectionExpression)variableInitializer.Initializer).Expression;
                }

                variableDeclarationStatement.Remove();
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                if (!_substitutes.TryGetValue(identifierExpression.GetFullName(), out var substitute))
                {
                    return;
                }

                identifierExpression.ReplaceWith(substitute.Clone());
            }
        }
    }
}
