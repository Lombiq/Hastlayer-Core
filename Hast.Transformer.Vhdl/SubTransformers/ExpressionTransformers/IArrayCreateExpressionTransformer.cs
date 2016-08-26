using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface IArrayCreateExpressionTransformer : IDependency
    {
        UnconstrainedArrayInstantiation CreateArrayInstantiation(ArrayCreateExpression expression);
        Value Transform(ArrayCreateExpression expression, IArchitectureComponent component);
    }
}
