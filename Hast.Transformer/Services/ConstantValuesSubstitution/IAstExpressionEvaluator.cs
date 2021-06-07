using Hast.Common.Interfaces;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    public interface IAstExpressionEvaluator : IDependency
    {
        dynamic EvaluateBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression);
        dynamic EvaluateCastExpression(CastExpression castExpression);
        dynamic EvaluateUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression);
    }
}
