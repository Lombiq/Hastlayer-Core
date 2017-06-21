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

                var parentTypeDefinition = arrayCreateExpression.FindFirstParentTypeDeclaration().Annotation<TypeDefinition>();
                var int32Assembly = typeof(int).Assembly;
                var int32TypeReference = new TypeReference(
                    "System",
                    "Int32",
                    parentTypeDefinition.Module,
                    new AssemblyNameReference(
                        int32Assembly.ShortName(),
                        new Version(int32Assembly.FullName.Split(',')[1].Substring(9))));
                int32TypeReference.IsValueType = true;
                var int32TypeInformation = new TypeInformation(int32TypeReference, int32TypeReference);

                // Setting the size argument, e.g. new int[] will be turned into new int[5].
                var sizeArgument = new PrimitiveExpression(initializerElements.Length);
                sizeArgument.AddAnnotation(int32TypeInformation);
                arrayCreateExpression.Arguments.Clear();
                arrayCreateExpression.Arguments.Add(sizeArgument);

                var parentAssignment = arrayCreateExpression.FindFirstParentOfType<AssignmentExpression>();

                // The array wasn't assigned to a variable or anything but rather directly passed to a method or
                // constructor. Thus first need to add a variable first to allow uniform processing later.
                if (parentAssignment == null)
                {
                    var variableName = "array" + Sha2456Helper.ComputeHash(arrayCreateExpression.GetFullName());
                    var parentStatement = arrayCreateExpression.FindFirstParentOfType<Statement>();
                    var typeInformation = arrayCreateExpression.Annotation<TypeInformation>();

                    var declarationType = new ComposedType { BaseType = arrayCreateExpression.Type.Clone() };
                    declarationType.ArraySpecifiers.Add(
                        new ArraySpecifier(((ArrayType)arrayCreateExpression.GetActualTypeReference()).Dimensions.Count));
                    var variableDeclaration = new VariableDeclarationStatement(declarationType, variableName);
                    variableDeclaration.AddAnnotation(typeInformation);
                    AstInsertionHelper.InsertStatementBefore(parentStatement, variableDeclaration);

                    var variableIdentifier = new IdentifierExpression(variableName);
                    variableIdentifier.AddAnnotation(typeInformation);

                    var newArrayCreateExpression = (ArrayCreateExpression)arrayCreateExpression.Clone();
                    arrayCreateExpression.CopyAnnotationsTo(newArrayCreateExpression);
                    parentAssignment = new AssignmentExpression(variableIdentifier, newArrayCreateExpression);

                    AstInsertionHelper.InsertStatementBefore(parentStatement, new ExpressionStatement(parentAssignment));

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
                        arrayCreateExpression.FindFirstParentOfType<Statement>(), 
                        elementAssignmentStatement);
                }
            }
        }
    }
}
