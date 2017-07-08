using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                if (objectCreateExpression.Parent is AssignmentExpression ||
                    objectCreateExpression.Type.GetFullName().IsDisplayClassName())
                {
                    return;
                }

                var variableIdentifier = VariableHelper
                    .DeclareAndReferenceVariable("object", objectCreateExpression, objectCreateExpression.Type);

                var assignment = new AssignmentExpression(variableIdentifier, objectCreateExpression.Clone());
                assignment.AddAnnotation(objectCreateExpression.Annotation<TypeInformation>());

                AstInsertionHelper.InsertStatementBefore(
                    objectCreateExpression.FindFirstParentStatement(), 
                    new ExpressionStatement(assignment));

                objectCreateExpression.ReplaceWith(variableIdentifier.Clone());
            }
        }
    }
}
