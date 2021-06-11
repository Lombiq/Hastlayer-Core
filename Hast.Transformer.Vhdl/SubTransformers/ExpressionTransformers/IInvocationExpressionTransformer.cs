using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    /// <summary>
    /// A sub-transformer specifically for <see cref="InvocationExpression"/>s.
    /// </summary>
    public interface IInvocationExpressionTransformer : IDependency
    {
        /// <summary>
        /// Transforms the <paramref name="expression"/> into VHDL code.
        /// </summary>
        IVhdlElement TransformInvocationExpression(
            InvocationExpression expression,
            ICollection<ITransformedInvocationParameter> transformedParameters,
            ISubTransformerContext context);
    }
}
