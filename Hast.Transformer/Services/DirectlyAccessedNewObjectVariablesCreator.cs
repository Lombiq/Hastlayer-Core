using System;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;

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

                var typeName = objectCreateExpression.Type.GetFullName();
                if (objectCreateExpression.Parent is AssignmentExpression ||
                    typeName.IsDisplayClassName()||
                    // Omitting Funcs for now, as those are used in parallel code with Tasks and handled separately.
                    typeName == "System.Func`2")
                {
                    return;
                }

                var variableIdentifier = VariableHelper
                    .DeclareAndReferenceVariable("object", objectCreateExpression, objectCreateExpression.Type);

                var assignment = new AssignmentExpression(variableIdentifier, objectCreateExpression.Clone())
                    .WithAnnotation(objectCreateExpression.Annotation<TypeInformation>());

                AstInsertionHelper.InsertStatementBefore(
                    objectCreateExpression.FindFirstParentStatement(), 
                    new ExpressionStatement(assignment));

                objectCreateExpression.ReplaceWith(variableIdentifier.Clone());
            }
        }
    }
}
