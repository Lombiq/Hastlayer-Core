using System;
using System.Linq;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.Decompiler.ILAst;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    public class InstanceMethodsToStaticConverter : IInstanceMethodsToStaticConverter
    {
        public void ConvertInstanceMethodsToStatic(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new InstanceMethodsToStaticConvertingVisitor(syntaxTree));
        }


        private class InstanceMethodsToStaticConvertingVisitor : DepthFirstAstVisitor
        {
            private readonly SyntaxTree _syntaxTree;


            public InstanceMethodsToStaticConvertingVisitor(SyntaxTree syntaxTree)
            {
                _syntaxTree = syntaxTree;
            }


            public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
            {
                // Omitting DisplayClasses because those are handled separately.
                if (methodDeclaration.GetFullName().IsDisplayClassMemberName()) return;

                base.VisitMethodDeclaration(methodDeclaration);

                var parentType = methodDeclaration.FindFirstParentTypeDeclaration();

                // We only have to deal with instance methods of non-hardware entry point classes.
                if (methodDeclaration.HasModifier(Modifiers.Static) ||
                    methodDeclaration.IsHardwareEntryPointMember() ||
                    parentType.Members.Any(member => member.IsHardwareEntryPointMember()))
                {
                    return;
                }

                var parentTypeDefinition = parentType.Annotation<TypeDefinition>();

                var parentAstType = AstType.Create(parentType.GetFullName());
                parentAstType.AddAnnotation(parentTypeDefinition);

                // Making the method static.
                methodDeclaration.Modifiers = methodDeclaration.Modifiers | Modifiers.Static;

                // Adding a "@this" parameter and using that instead of the "this" reference.
                var thisParameter = new ParameterDeclaration(parentAstType, "this")
                    .WithAnnotation(new ParameterDefinition("this", ParameterAttributes.None, parentTypeDefinition));
                if (!methodDeclaration.Parameters.Any())
                {
                    methodDeclaration.Parameters.Add(thisParameter);
                }
                else
                {
                    methodDeclaration.Parameters.InsertBefore(methodDeclaration.Parameters.First(), thisParameter);
                }

                methodDeclaration.AcceptVisitor(new ThisReferenceChangingVisitor());

                // Changing consumer code of the method to use it as static with the new "@this" parameter.
                _syntaxTree.AcceptVisitor(new MethodCallChangingVisitor(
                    parentAstType,
                    parentType.GetFullName(),
                    methodDeclaration.Name));
            }


            private class ThisReferenceChangingVisitor : DepthFirstAstVisitor
            {
                public override void VisitThisReferenceExpression(ThisReferenceExpression thisReferenceExpression)
                {
                    base.VisitThisReferenceExpression(thisReferenceExpression);

                    var thisIdentifierExpression = new IdentifierExpression("this");
                    var typeInformation = thisReferenceExpression.Annotation<TypeInformation>();

                    if (typeInformation == null)
                    {
                        typeInformation = thisReferenceExpression
                            .FindFirstParentTypeDeclaration()
                            .Annotation<TypeDefinition>()
                            .ToTypeInformation();
                    }

                    thisIdentifierExpression.AddAnnotation(typeInformation);
                    thisIdentifierExpression.AddAnnotation(new ILVariable { Name = "this", Type = typeInformation.ExpectedType });
                    thisReferenceExpression.ReplaceWith(thisIdentifierExpression);
                }
            }

            private class MethodCallChangingVisitor : DepthFirstAstVisitor
            {
                private readonly AstType _parentAstType;
                private readonly string _methodParentFullName;
                private readonly string _methodName;


                public MethodCallChangingVisitor(AstType parentAstType, string methodParentFullName, string methodName)
                {
                    _parentAstType = parentAstType;
                    _methodParentFullName = methodParentFullName;
                    _methodName = methodName;
                }


                public override void VisitInvocationExpression(InvocationExpression invocationExpression)
                {
                    base.VisitInvocationExpression(invocationExpression);

                    var targetMemberReference = invocationExpression.Target as MemberReferenceExpression;

                    if (targetMemberReference == null) return;

                    var targetTypeReference = targetMemberReference.Target.GetActualTypeReference();
                    var isAffectedMethodCall =
                        (targetMemberReference.Target is ThisReferenceExpression ||
                            targetTypeReference != null &&
                            targetTypeReference.FullName == _methodParentFullName)
                        &&
                        targetMemberReference.MemberName == _methodName;

                    if (!isAffectedMethodCall) return;

                    var originalTarget = targetMemberReference.Target;
                    targetMemberReference.Target.ReplaceWith(new TypeReferenceExpression(_parentAstType.Clone()));

                    if (!invocationExpression.Arguments.Any())
                    {
                        invocationExpression.Arguments.Add(originalTarget);
                    }
                    else
                    {
                        invocationExpression.Arguments.InsertBefore(invocationExpression.Arguments.First(), originalTarget);
                    }
                }
            }
        }
    }
}
