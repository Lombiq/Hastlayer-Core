namespace ICSharpCode.NRefactory.CSharp
{
    public static class SimpleMemoryInvocationExpressionExtensions
    {
        /// <summary>
        /// Determines whether the invocation expression is a SimpleMemory object member invocation.
        /// </summary>
        public static bool IsSimpleMemoryInvocation(this InvocationExpression expression)
        {
            var targetMemberReference = expression.Target as MemberReferenceExpression;

            return
                targetMemberReference != null &&
                targetMemberReference.Target.Is<IdentifierExpression>(identifier =>
                    identifier.Identifier == expression.FindFirstParentOfType<MethodDeclaration>().GetSimpleMemoryParameterName());
        }
    }
}
