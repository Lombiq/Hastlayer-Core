using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Helpers;
using Hast.Transformer.Helpers;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.TypeSystem;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    public class MethodInliner : IMethodInliner
    {
        public void InlineMethods(SyntaxTree syntaxTree)
        {
            var inlinableMethods = new Dictionary<string, MethodDeclaration>();

            foreach (var method in syntaxTree.GetAllTypeDeclarations().SelectMany(type => type.Members.Where(member => member is MethodDeclaration)))
            {
                var isInlinableMethod = method.Attributes
                    .Any(attributeSection => attributeSection
                        .Attributes
                        .Any(attribute => attribute
                            .FindFirstChildOfType<MemberReferenceExpression>(expression => expression
                                .Target
                                .Is<TypeReferenceExpression>(type => type.GetFullName() == typeof(MethodImplOptions).FullName) &&
                                expression.MemberName == nameof(MethodImplOptions.AggressiveInlining)) != null));

                if (isInlinableMethod)
                {
                    if (method.ReturnType.Is<PrimitiveType>(type => type.KnownTypeCode == KnownTypeCode.Void))
                    {
                        throw new NotSupportedException("Only non-void methods can be inlined.");
                    }

                    inlinableMethods[method.GetFullName()] = (MethodDeclaration)method;
                }
            }

            // Gradually inlining methods in multiple passes through the whole syntax tree. Doing this in iterations
            // because an inlined method can call another inlined method and so forth, requiring recursive inlining.

            string codeOutput;
            var passCount = 0;
            const int maxPassCount = 1000;
            do
            {
                codeOutput = syntaxTree.ToString();

                syntaxTree.AcceptVisitor(new MethodCallChangingVisitor(inlinableMethods));

                passCount++;
            } while (codeOutput != syntaxTree.ToString() && passCount < maxPassCount);

            if (passCount == maxPassCount)
            {
                throw new InvalidOperationException(
                    "Method inlining needs more than " + maxPassCount +
                    " passes through the syntax tree. This most possibly indicates some error or the assembly being processed is exceptionally big.");
            }
        }


        private static string SuffixMethodIdentifier(string identifier, string methodIdentifierNameSuffix) =>
            identifier + "_" + methodIdentifierNameSuffix;

        private class MethodCallChangingVisitor : DepthFirstAstVisitor
        {
            private readonly Dictionary<string, MethodDeclaration> _inlinableMethods;


            public MethodCallChangingVisitor(Dictionary<string, MethodDeclaration> inlinableMethods)
            {
                _inlinableMethods = inlinableMethods;
            }


            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                var targetMemberReference = invocationExpression.Target as MemberReferenceExpression;

                if (targetMemberReference == null) return;

                var memberFullName = targetMemberReference.GetMemberFullName();

                if (string.IsNullOrEmpty(memberFullName))
                {
                    // Sometimes there is no annotation on targetMemberReference at all.
                    // MemberReferenceExpressionExtensions.FindMemberDeclaration() has logic to iteratively find the 
                    // first parent with a MemberReference annotation too, for the same reason.
                    memberFullName = invocationExpression.Annotation<MemberReference>()?.FullName;
                }

                if (string.IsNullOrEmpty(memberFullName) || !_inlinableMethods.TryGetValue(memberFullName, out var method))
                {
                    return;
                }

                var invocationParentStatement = invocationExpression.FindFirstParentStatement();

                // Creating a suffix to make all identifiers (e.g. variable names) inside the method unique once inlined.
                // Since the same method can be inlined multiple times in another method we also need to distinguish per
                // invocation.
                var methodIdentifierNameSuffix = Sha2456Helper.ComputeHash(memberFullName + invocationExpression.CreateNameForUnnamedNode());

                // Assigning all invocation arguments to newly created local variables which then will be used in the
                // inlined method's body.
                var argumentsEnumerator = invocationExpression.Arguments.GetEnumerator();
                foreach (var parameter in method.Parameters)
                {
                    argumentsEnumerator.MoveNext();


                    var variableReference = VariableHelper.DeclareAndReferenceVariable(
                        SuffixMethodIdentifier(parameter.Name, methodIdentifierNameSuffix),
                        parameter.GetTypeInformationOrCreateFromActualTypeReference(),
                        parameter.Type,
                        invocationParentStatement);

                    AstInsertionHelper.InsertStatementBefore(
                        invocationParentStatement,
                        new ExpressionStatement(new AssignmentExpression(
                            variableReference,
                            argumentsEnumerator.Current.Clone())));
                }

                // Creating variable for the method's return value.
                var returnVariableReference = VariableHelper.DeclareAndReferenceVariable(
                    SuffixMethodIdentifier("return", methodIdentifierNameSuffix),
                    method.GetTypeInformationOrCreateFromActualTypeReference(),
                    method.ReturnType,
                    invocationParentStatement);

                // Preparing and adding the method's body inline.
                var inlinedBody = (BlockStatement)method.Body.Clone();
                inlinedBody.AcceptVisitor(new MethodBodyAdaptingVisitor(methodIdentifierNameSuffix, returnVariableReference));

                foreach (var statement in inlinedBody.Statements)
                {
                    AstInsertionHelper.InsertStatementBefore(
                        invocationParentStatement,
                        statement.Clone()); 
                }

                // The invocation now can be replaced with a reference to the return variable.
                invocationExpression.ReplaceWith(returnVariableReference);
            }
        }

        private class MethodBodyAdaptingVisitor : DepthFirstAstVisitor
        {
            private readonly string _methodIdentifierNameSuffix;
            private readonly IdentifierExpression _returnVariableReference;


            public MethodBodyAdaptingVisitor(
                string methodIdentifierNameSuffix, 
                IdentifierExpression returnVariableReferenc)
            {
                _methodIdentifierNameSuffix = methodIdentifierNameSuffix;
                _returnVariableReference = returnVariableReferenc;
            }


            public override void VisitReturnStatement(ReturnStatement returnStatement)
            {
                base.VisitReturnStatement(returnStatement);

                returnStatement.ReplaceWith(new ExpressionStatement(new AssignmentExpression(
                    _returnVariableReference.Clone(),
                    returnStatement.Expression.Clone())));
            }

            public override void VisitIdentifier(Identifier identifier)
            {
                base.VisitIdentifier(identifier);

                identifier.Name = SuffixMethodIdentifier(identifier.Name, _methodIdentifierNameSuffix);
            }
        }
    }
}
