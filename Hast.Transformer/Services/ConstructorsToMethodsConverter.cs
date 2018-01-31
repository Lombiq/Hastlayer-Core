using System;
using System.Linq;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    // Maybe this would be better suitable in Hast.Transformer.Vhdl since it might not be interesting for every hardware
    // description language. But then we'd need to run IInstanceMethodsToStaticConverter again to make constructor
    // methods static too. 
    public class ConstructorsToMethodsConverter : IConstructorsToMethodsConverter
    {
        public void ConvertConstructorsToMethods(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new ConstructorConvertingVisitor());
        }


        private class ConstructorConvertingVisitor : DepthFirstAstVisitor
        {
            public override void VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration)
            {
                base.VisitConstructorDeclaration(constructorDeclaration);

                var method = MethodDeclarationFactory.CreateMethod(
                    name: constructorDeclaration.Name,
                    annotations: constructorDeclaration.Annotations,
                    attributes: constructorDeclaration.Attributes,
                    parameters: constructorDeclaration.Parameters,
                    body: constructorDeclaration.Body,
                    returnType: new PrimitiveType("void"));

                // If the type has no base type then remove the automatically added base.ctor() call from the 
                // constructor as it won't reference anything transformable.
                if (!constructorDeclaration.FindFirstParentTypeDeclaration().BaseTypes.Any())
                {
                    method.Body
                        .OfType<ExpressionStatement>()
                        .SingleOrDefault(statement =>
                        {
                            var invocation = statement.Expression as InvocationExpression;

                            return
                                invocation != null &&
                                invocation.Target.Is<MemberReferenceExpression>(reference => reference.MemberName.IsConstructorName());
                        })?.Remove(); 
                }

                // If there is a constructor initializer (like Ctor() : this(bla)) then handle that too by adding an
                // explicit call.
                if (constructorDeclaration.Initializer != ConstructorInitializer.Null)
                {
                    if (constructorDeclaration.Initializer.ConstructorInitializerType != ConstructorInitializerType.This)
                    {
                        throw new NotSupportedException(
                            "Only this() constructor initializers are supported. Unsupported constructor: " +
                            Environment.NewLine +
                            constructorDeclaration.ToString());
                    }

                    var invocation = new InvocationExpression(
                        new MemberReferenceExpression(new ThisReferenceExpression(), constructorDeclaration.Name),
                        constructorDeclaration.Initializer.Arguments.Select(argument => argument.Clone()));

                    invocation.AddAnnotation(constructorDeclaration.Initializer.Annotation<MethodDefinition>());

                    var invocationStatement = new ExpressionStatement(invocation);
                    if (method.Body.Any())
                    {
                        AstInsertionHelper.InsertStatementBefore(method.Body.First(), invocationStatement);
                    }
                    else
                    {
                        method.Body.Add(invocationStatement);
                    }
                }

                constructorDeclaration.ReplaceWith(method);
            }
        }
    }
}
