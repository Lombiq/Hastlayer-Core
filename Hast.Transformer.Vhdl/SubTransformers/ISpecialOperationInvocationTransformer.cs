using System.Collections.Generic;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Transformer for dealing with special Hastlayer-supported computational operations.
    /// </summary>
    public interface ISpecialOperationInvocationTransformer : IDependency
    {
        bool IsSpecialOperationInvocation(InvocationExpression expression);

        IVhdlElement TransformSpecialOperationInvocation(
            InvocationExpression expression,
            IEnumerable<IVhdlElement> transformedParameters,
            ISubTransformerContext context);
    }
}
