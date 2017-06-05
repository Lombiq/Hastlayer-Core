using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Helpers
{
    internal static class ConstantValueSubstitutionHelper
    {
        public static bool IsInWhile(AstNode node) => node.IsIn<WhileStatement>();

        public static bool IsMethodInvocation(MemberReferenceExpression memberReferenceExpression) =>
            memberReferenceExpression.Parent.Is<InvocationExpression>(invocation => invocation.Target == memberReferenceExpression);

        public static ParameterDeclaration FindConstructorParameterForPassedExpression(
            ObjectCreateExpression objectCreateExpression,
            Expression passedExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable) =>
            FindParameterForExpressionPassedToCall(objectCreateExpression, objectCreateExpression.Arguments, passedExpression, typeDeclarationLookupTable);

        public static ParameterDeclaration FindMethodParameterForPassedExpression(
            InvocationExpression invocationExpression,
            Expression passedExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable) =>
            FindParameterForExpressionPassedToCall(invocationExpression, invocationExpression.Arguments, passedExpression, typeDeclarationLookupTable);


        // This could be optimized not to look up everything every time when called from VisitObjectCreateExpression()
        // and VisitInvocationExpression().
        private  static ParameterDeclaration FindParameterForExpressionPassedToCall(
            Expression callExpression,
            AstNodeCollection<Expression> invocationArguments,
            Expression passedExpression,
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var methodDefinition = callExpression.Annotation<MethodDefinition>();

            if (methodDefinition == null) return null;

            var targetFullName = callExpression.GetFullName();

            var parameters = ((MethodDeclaration)typeDeclarationLookupTable
                .Lookup(methodDefinition.DeclaringType.FullName)
                .Members
                .Single(member => member.GetFullName() == targetFullName))
                .Parameters
                .ToList();

            var arguments = invocationArguments.ToList();
            var argumentIndex = arguments.FindIndex(argumentExpression => argumentExpression == passedExpression);

            // Depending on whether a @this parameter was added to the method or used during invocation we need to
            // adjust the argument's index if there is a mismatch between the invocation and the method.
            if (parameters.Count < arguments.Count) argumentIndex--;
            else if (parameters.Count > arguments.Count) argumentIndex++;

            return parameters[argumentIndex];
        }
    }
}
