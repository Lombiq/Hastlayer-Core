namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class BinaryOperatorExpressionExtensions
    {
        public static TypeReference GetResultTypeReference(this BinaryOperatorExpression expression)
        {
            var resultType = expression.GetActualType(true);
            if (resultType == null)
            {
                resultType = expression.FindFirstNonParenthesizedExpressionParent().GetActualType();
            }

            return resultType;
        }
    }
}
