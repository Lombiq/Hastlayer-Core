namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class BinaryOperatorExpressionExtensions
    {
        public static TypeReference GetResultTypeReference(this BinaryOperatorExpression expression)
        {
            var resultTypeReference = expression.GetActualTypeReference(true);
            if (resultTypeReference == null)
            {
                resultTypeReference = expression.FindFirstNonParenthesizedExpressionParent().GetActualTypeReference();
            }

            return resultTypeReference;
        }
    }
}
