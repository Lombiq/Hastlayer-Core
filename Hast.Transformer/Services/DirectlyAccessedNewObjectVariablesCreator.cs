using System;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.CSharp;

namespace Hast.Transformer.Services
{
    public class DirectlyAccessedNewObjectVariablesCreator : IDirectlyAccessedNewObjectVariablesCreator
    {
        public void CreateVariablesForDirectlyAccessedNewObjects(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new DirectlyAccessedNewObjectVariableCreatingVisitor());
        }


        private class DirectlyAccessedNewObjectVariableCreatingVisitor : DepthFirstAstVisitor
        {
            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                HandleExpression(objectCreateExpression, objectCreateExpression.Type);
            }

            public override void VisitDefaultValueExpression(DefaultValueExpression defaultValueExpression)
            {
                base.VisitDefaultValueExpression(defaultValueExpression);

                // Handling cases like the one below, where Fix64 is a struct without a parameterless ctor:
                // public static Fix64 Zero() => new Fix64();

                HandleExpression(defaultValueExpression, defaultValueExpression.Type);
            }


            private static void HandleExpression(Expression expression, AstType astType)
            {
                var typeName = astType.GetFullName();
                if (expression.Parent is AssignmentExpression ||
                    typeName.IsDisplayOrClosureClassName() ||
                    // Omitting Funcs for now, as those are used in parallel code with Tasks and handled separately.
                    typeName == "System.Func`2")
                {
                    return;
                }

                var variableIdentifier = VariableHelper
                    .DeclareAndReferenceVariable("object", expression, astType);

                var assignment = new AssignmentExpression(variableIdentifier, expression.Clone())
                    .WithAnnotation(expression.Annotation<TypeInformation>());

                AstInsertionHelper.InsertStatementBefore(
                    expression.FindFirstParentStatement(),
                    new ExpressionStatement(assignment));

                expression.ReplaceWith(variableIdentifier.Clone());
            }
        }
    }
}
