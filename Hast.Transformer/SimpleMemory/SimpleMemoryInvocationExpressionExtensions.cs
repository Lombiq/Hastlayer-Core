using Mono.Cecil;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class SimpleMemoryInvocationExpressionExtensions
    {
        /// <summary>
        /// Determines whether the invocation expression is a SimpleMemory object member invocation.
        /// </summary>
        public static bool IsSimpleMemoryInvocation(this InvocationExpression expression) =>
            expression.Annotation<MethodReference>()?.DeclaringType.IsSimpleMemory() == true;
    }
}
