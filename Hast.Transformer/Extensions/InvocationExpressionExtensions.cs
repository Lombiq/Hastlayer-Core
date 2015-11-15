using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Ast;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class InvocationExpressionExtensions
    {
        /// <summary>
        /// Retrieves the return type of the method that was invoked in an invokation expression.
        /// </summary>
        public static TypeReference GetReturnType(this InvocationExpression expression)
        {
            // Looking up the type information that will tell us what the return type of the invokation is. 
            // This might be some nodes up if e.g. there is an immediate cast expression.
            AstNode currentNode = expression;
            while (currentNode.Annotation<TypeInformation>() == null)
            {
                currentNode = currentNode.Parent;
            }

            return currentNode.GetActualType();
        }
    }
}
