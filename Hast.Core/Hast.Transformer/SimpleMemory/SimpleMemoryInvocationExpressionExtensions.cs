using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class SimpleMemoryInvocationExpressionExtensions
    {
        /// <summary>
        /// Determines whether the invocation expression is a SimpleMemory object member invocation.
        /// </summary>
        public static bool IsSimpleMemoryInvocation(this InvocationExpression expression)
        {
            var methodReference = expression.Annotation<MethodReference>();
            return methodReference != null && methodReference.DeclaringType.IsSimpleMemory();
        }
    }
}
