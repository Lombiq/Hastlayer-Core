using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface IArrayCreateExpressionTransformer : IDependency
    {
        UnconstrainedArrayInstantiation CreateArrayInstantiation(ArrayCreateExpression expression);

        IVhdlElement Transform(ArrayCreateExpression expression, ISubTransformerContext context);
    }
}
