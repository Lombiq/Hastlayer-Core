using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class MemberIdentifiersFixer : IMemberIdentifiersFixer
    {
        public void FixMemberIdentifiers(SyntaxTree syntaxTree) => syntaxTree.AcceptVisitor(new MemberIdentifiersFixingVisitor());

        private class MemberIdentifiersFixingVisitor : DepthFirstAstVisitor
        {
            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                var parent = identifierExpression.Parent;
                var identifier = identifierExpression.Identifier;

                IMember member;
                if (parent is InvocationExpression invocation && invocation.Target == identifierExpression)
                {
                    // A normal method invocation.
                    member = parent.GetMemberResolveResult()?.Member;
                }
                else if (identifierExpression
                    .GetResolveResult<MethodGroupResolveResult>()
                    ?.Methods
                    ?.Any(method => method.GetFullName().IsInlineCompilerGeneratedMethodName()) == true)
                {
                    // A reference to a DisplayClass member or compiler-generated method within a Task.Factory.StartNew
                    // call.
                    member = identifierExpression.GetResolveResult<MethodGroupResolveResult>()?.Methods.Single();
                }
                else if (identifierExpression.GetMemberResolveResult() is MemberResolveResult memberResolveResult)
                {
                    // A property access.
                    if (memberResolveResult.Member.Name != identifier)
                    {
                        return;
                    }

                    member = memberResolveResult.Member;
                }
                else
                {
                    return;
                }

                if (member.IsStatic)
                {
                    var typeResolveResult = new TypeResolveResult(member.DeclaringType);
                    var typeReferenceExpression =
                        new TypeReferenceExpression(TypeHelper.CreateAstType(member.DeclaringType))
                        .WithAnnotation(typeResolveResult);
                    var memberReference = new MemberReferenceExpression(typeReferenceExpression, identifier)
                        .WithAnnotation(new MemberResolveResult(typeResolveResult, member));
                    identifierExpression.ReplaceWith(memberReference);
                }
                else
                {
                    var thisResolveResult = new ThisResolveResult(member.DeclaringType);
                    var thisReferenceExpression = new ThisReferenceExpression().WithAnnotation(thisResolveResult);

                    var memberReference = new MemberReferenceExpression(thisReferenceExpression, identifier)
                        .WithAnnotation(new MemberResolveResult(thisResolveResult, member));
                    identifierExpression.ReplaceWith(memberReference);
                }
            }
        }
    }
}
