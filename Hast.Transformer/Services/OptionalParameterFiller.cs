using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class OptionalParameterFiller : IOptionalParameterFiller
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;

        public OptionalParameterFiller(ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory) => _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;

        public void FillOptionalParamters(SyntaxTree syntaxTree) => syntaxTree.AcceptVisitor(new OptionalParamtersFillingVisitor(_typeDeclarationLookupTableFactory.Create(syntaxTree)));

        private class OptionalParamtersFillingVisitor : DepthFirstAstVisitor
        {
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;

            public OptionalParamtersFillingVisitor(ITypeDeclarationLookupTable typeDeclarationLookupTable) => _typeDeclarationLookupTable = typeDeclarationLookupTable;

            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                if (!(invocationExpression.Target is MemberReferenceExpression memberReferenceExpression) ||
                    !memberReferenceExpression.IsMethodReference())
                {
                    return;
                }

                var method = (MethodDeclaration)memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable);

                if (method == null) return;

                FillOptionalParameters(invocationExpression.Arguments, method.Parameters);
            }

            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                var constructor = objectCreateExpression.FindConstructorDeclaration(_typeDeclarationLookupTable) as MethodDeclaration;

                if (constructor == null) return;

                // Need to skip the first "this" parameter.
                FillOptionalParameters(objectCreateExpression.Arguments, constructor.Parameters.Skip(1).ToList());
            }

            private static void FillOptionalParameters(
                ICollection<Expression> arguments,
                ICollection<ParameterDeclaration> parameters)
            {
                if (arguments.Count == parameters.Count) return;

                // All the remaining parameters are optional ones.
                var parametersArray = parameters.ToArray();
                for (int i = arguments.Count; i < parameters.Count; i++)
                {
                    arguments.Add(parametersArray[i].DefaultExpression.Clone());
                }
            }
        }
    }
}
