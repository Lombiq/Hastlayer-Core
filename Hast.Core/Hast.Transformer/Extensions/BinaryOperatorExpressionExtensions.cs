using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class BinaryOperatorExpressionExtensions
    {
        public static TypeReference GetResultTypeReference(this BinaryOperatorExpression expression)
        {
            var firstNonParenthesizedExpressionParent = expression.FindFirstNonParenthesizedExpressionParent();

            var resultTypeReference = expression.GetActualTypeReference(true);
            if (resultTypeReference == null)
            {
                resultTypeReference = firstNonParenthesizedExpressionParent.GetActualTypeReference();
            }

            return resultTypeReference;
        }
    }
}
