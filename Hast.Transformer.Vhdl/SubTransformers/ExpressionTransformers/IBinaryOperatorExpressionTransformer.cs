using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    /// <summary>
    /// A transformer specifically for binary operator expressions.
    /// </summary>
    public interface IBinaryOperatorExpressionTransformer : IDependency
    {
        /// <summary>
        /// Transforms binary operator expressions that can be executed in parallel.
        /// </summary>
        IEnumerable<IVhdlElement> TransformParallelBinaryOperatorExpressions(
              IEnumerable<IPartiallyTransformedBinaryOperatorExpression> partiallyTransformedExpressions,
              ISubTransformerContext context);

        /// <summary>
        /// Transforms regular binary operator expressions.
        /// </summary>
        IVhdlElement TransformBinaryOperatorExpression(
            IPartiallyTransformedBinaryOperatorExpression partiallyTransformedExpression,
            ISubTransformerContext context);
    }
}
