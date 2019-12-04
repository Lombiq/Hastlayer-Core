using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Helpers;
using Hast.Layer;
using Hast.Transformer.Helpers;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Mono.Cecil;

namespace Hast.Transformer.Services
{
    public class MethodInliner : IMethodInliner
    {
        public void InlineMethods(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            var additionalInlinableMethodsFullNames = configuration.TransformerConfiguration().AdditionalInlinableMethodsFullNames;
            var inlinableMethods = new Dictionary<string, MethodDeclaration>();

            foreach (var method in syntaxTree.GetAllTypeDeclarations().SelectMany(type => type.Members.Where(member => member is MethodDeclaration)))
            {
                var isInlinableMethod = 
                    additionalInlinableMethodsFullNames.Contains(method.GetFullName()) ||
                    method.Attributes
                        .Any(attributeSection => attributeSection
                            .Attributes
                            .Any(attribute => attribute
                                .FindFirstChildOfType<MemberReferenceExpression>(expression => expression
                                    .Target
                                    .Is<TypeReferenceExpression>(type => 
                                        type.GetFullName() == typeof(MethodImplOptions).FullName) &&
                                        expression.MemberName == nameof(MethodImplOptions.AggressiveInlining)) != null));

                if (isInlinableMethod)
                {
                    if (method.ReturnType.Is<PrimitiveType>(type => type.KnownTypeCode == KnownTypeCode.Void))
                    {
                        throw new NotSupportedException(
                            "The method " + method.GetFullName() + 
                            " can't be inlined, because that's only available for non-void methods.");
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

                var methodFullName = invocationExpression.GetTargetMemberFullName();

                if (string.IsNullOrEmpty(methodFullName) || !_inlinableMethods.TryGetValue(methodFullName, out var method))
                {
                    return;
                }

                var invocationParentStatement = invocationExpression.FindFirstParentStatement();

                // Creating a suffix to make all identifiers (e.g. variable names) inside the method unique once inlined.
                // Since the same method can be inlined multiple times in another method we also need to distinguish per
                // invocation.
                var methodIdentifierNameSuffix = Sha2456Helper.ComputeHash(methodFullName + invocationExpression.GetFullName());

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
                inlinedBody.AcceptVisitor(new MethodBodyAdaptingVisitor(methodIdentifierNameSuffix, returnVariableReference, methodFullName));

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
            private readonly string _methodFullName;

            private bool _aReturnStatementWasVisited;


            public MethodBodyAdaptingVisitor(
                string methodIdentifierNameSuffix, 
                IdentifierExpression returnVariableReferenc,
                string methodFullName)
            {
                _methodIdentifierNameSuffix = methodIdentifierNameSuffix;
                _returnVariableReference = returnVariableReferenc;
                _methodFullName = methodFullName;
            }


            public override void VisitReturnStatement(ReturnStatement returnStatement)
            {
                base.VisitReturnStatement(returnStatement);

                if (_aReturnStatementWasVisited)
                {
                    throw new NotSupportedException(
                        "Inlining methods with only a single return statement is supported. The method " +
                        _methodFullName + " contains more than one return statement.");
                }

                returnStatement.ReplaceWith(new ExpressionStatement(new AssignmentExpression(
                    _returnVariableReference.Clone(),
                    returnStatement.Expression.Clone())));

                _aReturnStatementWasVisited = true;
            }

            public override void VisitVariableInitializer(VariableInitializer variableInitializer)
            {
                base.VisitVariableInitializer(variableInitializer);

                variableInitializer.Name = SuffixMethodIdentifier(variableInitializer.Name, _methodIdentifierNameSuffix);
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                identifierExpression.Identifier = SuffixMethodIdentifier(identifierExpression.Identifier, _methodIdentifierNameSuffix);
            }
        }
    }
}
