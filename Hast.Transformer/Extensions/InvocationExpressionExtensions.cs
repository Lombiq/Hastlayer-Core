using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class InvocationExpressionExtensions
    {
        /// <summary>
        /// Retrieves the return type of the method that was invoked in an invocation expression.
        /// </summary>
        public static TypeReference GetReturnTypeReference(this InvocationExpression expression)
        {
            // Looking up the type information that will tell us what the return type of the invocation is. 
            // This might be some nodes up if e.g. there is an immediate cast expression.
            AstNode currentNode = expression;
            while (currentNode.Annotation<TypeInformation>() == null)
            {
                currentNode = currentNode.Parent;
            }

            return currentNode.GetActualTypeReference();
        }
    }
}
