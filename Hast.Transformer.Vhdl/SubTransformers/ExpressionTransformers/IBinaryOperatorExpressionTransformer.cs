using System.Collections.Generic;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface IBinaryOperatorExpressionTransformer : IDependency
    {
        IEnumerable<IVhdlElement> TransformParallelBinaryOperatorExpressions(
              IEnumerable<IPartiallyTransformedBinaryOperatorExpression> partiallyTransformedExpressions,
              ISubTransformerContext context);

        IVhdlElement TransformBinaryOperatorExpression(
            IPartiallyTransformedBinaryOperatorExpression partiallyTransformedExpression,
            ISubTransformerContext context);
    }
}
