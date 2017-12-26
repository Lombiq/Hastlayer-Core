using System.Collections.Generic;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface IInvocationExpressionTransformer : IDependency
    {
        IVhdlElement TransformInvocationExpression(
            InvocationExpression expression,
            IEnumerable<ITransformedInvocationParameter> transformedParameters,
            ISubTransformerContext context);
    }
}
