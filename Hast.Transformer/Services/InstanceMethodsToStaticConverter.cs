using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.IL;
using ICSharpCode.Decompiler.Semantics;
using System;
using System.Linq;

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
                if (methodDeclaration.GetFullName().IsDisplayOrClosureClassMemberName()) return;

                base.VisitMethodDeclaration(methodDeclaration);

                var parentType = methodDeclaration.FindFirstParentTypeDeclaration();

                // We only have to deal with instance methods of non-hardware entry point classes.
                if (methodDeclaration.HasModifier(Modifiers.Static) ||
                    methodDeclaration.IsHardwareEntryPointMember() ||
                    parentType.Members.Any(member => member.IsHardwareEntryPointMember()))
                {
                    return;
                }

                var parentTypeResolveResult = parentType.GetResolveResult<TypeResolveResult>();

                var parentAstType = AstType.Create(parentType.GetFullName());
                parentAstType.AddAnnotation(parentTypeResolveResult);

                // Making the method static.
                methodDeclaration.Modifiers |= Modifiers.Static;

                // Adding a "@this" parameter and using that instead of the "this" reference.
                var thisParameter = new ParameterDeclaration(parentAstType, "this")
                    .WithAnnotation(new ILVariableResolveResult(
                        new ILVariable(VariableKind.Parameter, parentTypeResolveResult.Type) { Name = "this" }));
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

                    var thisVariableResolveResult = new ILVariableResolveResult(
                        new ILVariable(
                            VariableKind.Parameter,
                            thisReferenceExpression.GetResolveResult().Type)
                        {
                            Name = "this"
                        });
                    var thisIdentifierExpression = new IdentifierExpression("this").WithAnnotation(thisVariableResolveResult);
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

                    var targetType = targetMemberReference.Target.GetActualType();
                    var isAffectedMethodCall =
                        (targetMemberReference.Target is ThisReferenceExpression ||
                            targetType != null &&
                            targetType.FullName == _methodParentFullName)
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
