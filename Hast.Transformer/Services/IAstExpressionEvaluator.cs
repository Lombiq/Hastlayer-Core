using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Services
{
    public interface IAstExpressionEvaluator : IDependency
    {
        dynamic EvaluateBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression);
        dynamic EvaluateCastExpression(CastExpression castExpression);
    }
}
