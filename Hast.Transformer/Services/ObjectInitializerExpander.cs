using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services
{
    public class ObjectInitializerExpander : IObjectInitializerExpander
    {
        public void ExpandObjectInitializers(SyntaxTree syntaxTree) => syntaxTree.AcceptVisitor(new ObjectInitializerExpanderVisitor());

        private class ObjectInitializerExpanderVisitor : DepthFirstAstVisitor
        {
            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                if (!objectCreateExpression.Initializer.Elements.Any()) return;

                // At this point there will be a parent assignment due to IDirectlyAccessedNewObjectVariablesCreator.
                var parentAssignment = objectCreateExpression
                    .FindFirstParentOfType<AssignmentExpression>(assignment => assignment.Right == objectCreateExpression);

                var parentStatement = objectCreateExpression.FindFirstParentStatement();

                foreach (var initializerElement in objectCreateExpression.Initializer.Elements)
                {
                    var namedInitializerExpression = initializerElement as NamedExpression;

                    if (namedInitializerExpression == null)
                    {
                        throw new NotSupportedException(
                            "Object initializers can only contain named expressions (i.e. \"Name = expression\" pairs)."
                            .AddParentEntityName(objectCreateExpression));
                    }

                    var memberReference = new MemberReferenceExpression(parentAssignment.Left.Clone(), namedInitializerExpression.Name);
                    namedInitializerExpression.CopyAnnotationsTo(memberReference);
                    var propertyAssignmentStatement = new ExpressionStatement(new AssignmentExpression(
                        left: memberReference,
                        right: namedInitializerExpression.Expression.Clone()));

                    AstInsertionHelper.InsertStatementAfter(parentStatement, propertyAssignmentStatement);

                    initializerElement.Remove();
                }
            }
        }
    }
}
