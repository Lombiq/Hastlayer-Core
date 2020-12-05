using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hast.Transformer.Vhdl.Verifiers
{
    public class CompilerGeneratedClassesVerifier : ICompilerGeneratedClassesVerifier
    {
        public void VerifyCompilerGeneratedClasses(SyntaxTree syntaxTree)
        {
            var compilerGeneratedClasses = syntaxTree
                .GetAllTypeDeclarations()
                .Where(type => type.GetFullName().IsDisplayOrClosureClassName());

            foreach (var compilerGeneratedClass in compilerGeneratedClasses)
            {
                var fields = compilerGeneratedClass.Members.OfType<FieldDeclaration>()
                    .OrderBy(field => field.Variables.Single().Name)
                    .ToDictionary(field => field.Variables.Single().Name);

                foreach (var method in compilerGeneratedClass.Members.OfType<MethodDeclaration>())
                {
                    // Adding parameters for every field that the method used, in alphabetical order, and changing field
                    // references to parameter references.

                    Action<MemberReferenceExpression> memberReferenceExpressionProcessor = memberReferenceExpression =>
                    {
                        if (!memberReferenceExpression.IsFieldReference()) return;

                        var fullName = memberReferenceExpression.GetMemberResolveResult().GetFullName();

                        var field = fields.Values
                            .SingleOrDefault(f => f.GetMemberResolveResult().GetFullName() == fullName);

                        // The field won't be on the compiler-generated class if the member reference accesses a
                        // user-defined type's field.
                        if (field == null) return;

                        // Is the field assigned to? Because we don't support that currently, since with it being
                        // converted to a parameter we'd need to return its value and assign it to the caller's
                        // variable. Maybe we'll allow this with static field support, but not for lambdas used in
                        // parallelized expressions (since that would require concurrent access too).
                        var isAssignedTo =
                            // The field is directly assigned to.
                            (memberReferenceExpression.Parent is AssignmentExpression &&
                            ((AssignmentExpression)memberReferenceExpression.Parent).Left == memberReferenceExpression)
                            ||
                            // The field's indexed element is assigned to.
                            (memberReferenceExpression.Parent is IndexerExpression &&
                            memberReferenceExpression.Parent.Parent is AssignmentExpression &&
                            ((AssignmentExpression)memberReferenceExpression.Parent.Parent).Left == memberReferenceExpression.Parent);
                        if (isAssignedTo)
                        {
                            throw new NotSupportedException(
                                "It's not supported to modify the content of a variable coming from the parent scope in a lambda expression. " +
                                "Pass arguments instead. Affected method: " + Environment.NewLine + method);
                        }
                    };

                    method.AcceptVisitor(new MemberReferenceExpressionVisitingVisitor(memberReferenceExpressionProcessor));
                }
            }
        }

        private class MemberReferenceExpressionVisitingVisitor : DepthFirstAstVisitor
        {
            private readonly Action<MemberReferenceExpression> _expressionProcessor;

            public MemberReferenceExpressionVisitingVisitor(Action<MemberReferenceExpression> expressionProcessor)
            {
                _expressionProcessor = expressionProcessor;
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);
                _expressionProcessor(memberReferenceExpression);
            }
        }
    }
}
