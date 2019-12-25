using ICSharpCode.Decompiler.CSharp.Resolver;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Linq;

namespace Hast.Transformer.Services
{
    public class MemberIdentifiersFixer : IMemberIdentifiersFixer
    {
        public void FixMemberIdentifiers(SyntaxTree syntaxTree)
        {
            syntaxTree.AcceptVisitor(new MemberIdentifiersFixingVisitor());
        }


        private class MemberIdentifiersFixingVisitor : DepthFirstAstVisitor
        {
            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                var parent = identifierExpression.Parent;
                var identifier = identifierExpression.Identifier;

                var member = parent.GetResolveResult<MemberResolveResult>()?.Member;
                ThisResolveResult thisResolveResult;
                if (parent is InvocationExpression invocation && invocation.Target == identifierExpression)
                {
                    // A normal method invocation.
                    thisResolveResult = new ThisResolveResult(member.DeclaringType);
                }
                else if (identifierExpression
                    .GetResolveResult<MethodGroupResolveResult>()
                    ?.Methods
                    .Single()
                    .GetFullName()
                    .IsInlineCompilerGeneratedMethodName() == true)
                {
                    // A reference to a DisplayClass member or compiler-generated method within a Task.Factory.StartNew
                    // call.
                    member = identifierExpression.GetResolveResult<MethodGroupResolveResult>()?.Methods.Single();
                    thisResolveResult = new ThisResolveResult(member.DeclaringType);
                }
                else if (identifierExpression.GetResolveResult<MemberResolveResult>() is MemberResolveResult memberResolveResult)
                {
                    // A property access.
                    if (memberResolveResult.Member.Name != identifier)
                    {
                        return;
                    }

                    member = memberResolveResult.Member;
                    thisResolveResult = memberResolveResult.TargetResult as ThisResolveResult ?? new ThisResolveResult(member.DeclaringType);
                }
                else
                {
                    return;
                }

                var thisReferenceExpression = new ThisReferenceExpression().WithAnnotation(thisResolveResult);

                var memberReference = new MemberReferenceExpression(thisReferenceExpression, identifier)
                    .WithAnnotation(new MemberResolveResult(thisResolveResult, member));
                identifierExpression.ReplaceWith(memberReference);
            }
        }
    }
}
