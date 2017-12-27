using Hast.Transformer.Helpers;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services
{
    public class OperatorsToMethodsConverter : IOperatorsToMethodsConverter
    {
        public void ConvertOperatorsToMethods(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new OperatorConvertingVisitor());
        }


        private class OperatorConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitOperatorDeclaration(OperatorDeclaration operatorDeclaration)
            {
                base.VisitOperatorDeclaration(operatorDeclaration);

                var method = MethodDeclarationFactory.CreateMethod(
                    name: operatorDeclaration.Name,
                    annotations: operatorDeclaration.Annotations,
                    attributes: operatorDeclaration.Attributes,
                    parameters: operatorDeclaration.Parameters,
                    body: operatorDeclaration.Body,
                    returnType: operatorDeclaration.ReturnType);

                method.Modifiers = operatorDeclaration.Modifiers;

                operatorDeclaration.ReplaceWith(method);
            }
        }
    }
}
