using System;
using System.Linq;
using System.Reflection;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    public class ArrayInitializerExpander : IArrayInitializerExpander
    {
        public void ExpandArrayInitializers(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new ArrayInitializerExpanderVisitor());
        }


        private class ArrayInitializerExpanderVisitor : DepthFirstAstVisitor
        {
            public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
            {
                base.VisitArrayCreateExpression(arrayCreateExpression);

                if (!arrayCreateExpression.Initializer.Elements.Any()) return;

                var initializerElements = arrayCreateExpression.Initializer.Elements.ToArray();
                arrayCreateExpression.Initializer.Elements.Clear();

                var int32TypeInformation = TypeHelper.CreateInt32TypeInformation(arrayCreateExpression);

                // Setting the size argument, e.g. new int[] will be turned into new int[5].
                var sizeArgument = new PrimitiveExpression(initializerElements.Length);
                sizeArgument.AddAnnotation(int32TypeInformation);
                arrayCreateExpression.Arguments.Clear();
                arrayCreateExpression.Arguments.Add(sizeArgument);

                var parentAssignment = arrayCreateExpression
                    .FindFirstParentOfType<AssignmentExpression>(assignment => assignment.Right == arrayCreateExpression);

                // The array wasn't assigned to a variable or anything but rather directly passed to a method or
                // constructor. Thus first we need to add a variable to allow uniform processing later.
                if (parentAssignment == null)
                {
                    var variableIdentifier = VariableHelper.DeclareAndReferenceArrayVariable(
                        arrayCreateExpression,
                        arrayCreateExpression.Type,
                        arrayCreateExpression.GetActualTypeReference());

                    var newArrayCreateExpression = (ArrayCreateExpression)arrayCreateExpression.Clone();
                    arrayCreateExpression.CopyAnnotationsTo(newArrayCreateExpression);
                    parentAssignment = new AssignmentExpression(variableIdentifier, newArrayCreateExpression);
                    parentAssignment.AddAnnotation(arrayCreateExpression.Annotation<TypeInformation>());

                    AstInsertionHelper.InsertStatementBefore(
                        arrayCreateExpression.FindFirstParentStatement(), 
                        new ExpressionStatement(parentAssignment));

                    arrayCreateExpression.ReplaceWith(variableIdentifier.Clone());
                    arrayCreateExpression = newArrayCreateExpression;
                }

                for (int i = initializerElements.Length - 1; i >= 0; i--)
                {
                    var indexArgument = new PrimitiveExpression(i);
                    indexArgument.AddAnnotation(int32TypeInformation);

                    var elementAssignmentStatement = new ExpressionStatement(new AssignmentExpression(
                        left: new IndexerExpression(
                            target: parentAssignment
                                .Left // This should be the IdentifierExpression that the array was assigned to.
                                .Clone(),
                            arguments: indexArgument),
                        right: initializerElements[i]
                        ));

                    AstInsertionHelper.InsertStatementAfter(
                        arrayCreateExpression.FindFirstParentStatement(), 
                        elementAssignmentStatement);
                }
            }
        }
    }
}
