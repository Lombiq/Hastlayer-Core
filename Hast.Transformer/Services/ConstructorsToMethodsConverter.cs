using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;

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

                var constructorMethod = new MethodDeclaration();

                foreach (var annotation in constructorDeclaration.Annotations)
                {
                    constructorMethod.AddAnnotation(annotation);
                }

                foreach (var parameter in constructorDeclaration.Parameters)
                {
                    constructorMethod.Parameters.Add(parameter.Clone());
                }

                constructorMethod.Name = constructorDeclaration.Name;
                constructorMethod.Body = (BlockStatement)constructorDeclaration.Body.Clone();
                constructorMethod.ReturnType = new PrimitiveType("void");

                // If the type has no base type then remove the automatically added base.ctor() call from the 
                // constructor as it won't reference anything transformable.
                if (!constructorDeclaration.FindFirstParentTypeDeclaration().BaseTypes.Any())
                {
                    constructorMethod.Body
                        .OfType<ExpressionStatement>()
                        .SingleOrDefault(statement =>
                        {
                            var invocation = statement.Expression as InvocationExpression;

                            return
                                invocation != null &&
                                invocation.Target.Is<MemberReferenceExpression>(reference => reference.MemberName.IsConstructorName());
                        })?.Remove(); 
                }

                constructorDeclaration.ReplaceWith(constructorMethod);
            }
        }
    }
}
