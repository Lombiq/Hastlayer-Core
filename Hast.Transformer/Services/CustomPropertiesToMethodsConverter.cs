using System.Linq;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    public class CustomPropertiesToMethodsConverter : ICustomPropertiesToMethodsConverter
    {
        public void ConvertCustomPropertiesToMethods(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new CustomPropertiesConvertingVisitor(syntaxTree));
        }


        private class CustomPropertiesConvertingVisitor : DepthFirstAstVisitor
        {
            private readonly SyntaxTree _syntaxTree;


            public CustomPropertiesConvertingVisitor(SyntaxTree syntaxTree)
            {
                _syntaxTree = syntaxTree;
            }


            public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
            {
                base.VisitPropertyDeclaration(propertyDeclaration);

                // We only care about properties with custom implemented getters and/or setters. If the getter and 
                // the setter is empty then it's an auto-property. If the getter is compiler-generated then it's also
                // an auto-property (a read-only one).
                if (!propertyDeclaration.Getter.Body.Any() && !propertyDeclaration.Setter.Body.Any() ||
                    propertyDeclaration.Getter.Attributes.Any(attributeSection => 
                        attributeSection.Attributes.Any(attribute => 
                            attribute.Type.Is<SimpleType>(type => type.Identifier == "CompilerGenerated"))))
                {
                    return;
                }


                var parentType = propertyDeclaration.FindFirstParentTypeDeclaration();

                var getter = propertyDeclaration.Getter;
                if (getter.Body.Any())
                {
                    var getterMethod = MethodDeclarationFactory.CreateMethod(
                        name: getter.Annotation<MethodDefinition>().Name,
                        annotations: getter.Annotations,
                        attributes: getter.Attributes,
                        parameters: Enumerable.Empty<ParameterDeclaration>(),
                        body: getter.Body,
                        returnType: propertyDeclaration.ReturnType);
                    parentType.Members.Add(getterMethod); 
                }

                var setter = propertyDeclaration.Setter;
                if (setter.Body.Any())
                {
                    var valueParameter = new ParameterDeclaration(propertyDeclaration.ReturnType.Clone(), "value");
                    valueParameter.AddAnnotation(new ParameterDefinition(
                        "value", ParameterAttributes.None, propertyDeclaration.Annotation<PropertyDefinition>().PropertyType));
                    var setterMethod = MethodDeclarationFactory.CreateMethod(
                        name: setter.Annotation<MethodDefinition>().Name,
                        annotations: setter.Annotations,
                        attributes: setter.Attributes,
                        parameters: new[] { valueParameter },
                        body: setter.Body,
                        returnType: new PrimitiveType("void"));
                    parentType.Members.Add(setterMethod); 
                }

                // Changing consumer code of the property to use it as methods.
                _syntaxTree.AcceptVisitor(new PropertyAccessChangingVisitor(
                    propertyDeclaration.GetFullNameWithUnifiedPropertyName()));

                propertyDeclaration.Remove();
            }


            private class PropertyAccessChangingVisitor : DepthFirstAstVisitor
            {
                private readonly string _unifiedPropertyName;


                public PropertyAccessChangingVisitor(string unifiedPropertyName)
                {
                    _unifiedPropertyName = unifiedPropertyName;
                }


                public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
                {
                    base.VisitMemberReferenceExpression(memberReferenceExpression);

                    if (memberReferenceExpression.GetMemberFullName() != _unifiedPropertyName) return;

                    // On property accesses there is a PropertyDefinition and MethodDefinition, sometimes MethodReference
                    // annotation. We need to use the latter to convert the property access into a method call.
                    memberReferenceExpression.RemoveAnnotations<PropertyDefinition>();
                    var methodReference = memberReferenceExpression.Annotation<MethodReference>();
                    memberReferenceExpression.MemberName = methodReference.Name;
                    var invocation = new InvocationExpression(memberReferenceExpression.Clone());
                    invocation.AddAnnotation(methodReference);
                    if (methodReference.IsDefinition && ((MethodDefinition)methodReference).IsSetter ||
                        !methodReference.IsDefinition && methodReference.Name.StartsWith("set_"))
                    {
                        var parentAssignment = (AssignmentExpression)memberReferenceExpression.Parent;
                        invocation.Arguments.Add(parentAssignment.Right.Clone());
                        parentAssignment.ReplaceWith(invocation);
                    }
                    else
                    {
                        memberReferenceExpression.ReplaceWith(invocation);
                    }
                }
            }
        }
    }
}
