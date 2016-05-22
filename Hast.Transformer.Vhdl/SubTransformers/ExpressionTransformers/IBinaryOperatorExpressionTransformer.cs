using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface IBinaryOperatorExpressionTransformer : IDependency
    {
        IVhdlElement TransformBinaryOperatorExpression(
            BinaryOperatorExpression expression,
            IVhdlElement leftTransformed,
            IVhdlElement rightTransformed,
            ISubTransformerContext context);
    }
}
