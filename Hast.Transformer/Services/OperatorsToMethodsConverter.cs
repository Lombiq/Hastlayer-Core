﻿using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;

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
                    operatorDeclaration.Name,
                    operatorDeclaration.Annotations,
                    operatorDeclaration.Parameters,
                    operatorDeclaration.Body,
                    operatorDeclaration.ReturnType);

                method.Modifiers = operatorDeclaration.Modifiers;

                operatorDeclaration.ReplaceWith(method);
            }
        }
    }
}
