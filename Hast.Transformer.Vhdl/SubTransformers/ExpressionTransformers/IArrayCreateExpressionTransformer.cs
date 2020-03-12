using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface IArrayCreateExpressionTransformer : IDependency
    {
        UnconstrainedArrayInstantiation CreateArrayInstantiation(
            ArrayCreateExpression expression, 
            IVhdlTransformationContext context);

        IVhdlElement Transform(ArrayCreateExpression expression, ISubTransformerContext context);
    }
}
