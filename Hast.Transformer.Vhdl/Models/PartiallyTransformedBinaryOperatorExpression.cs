using Hast.VhdlBuilder.Representation;
using ICSharpCode.Decompiler.CSharp;

namespace Hast.Transformer.Vhdl.Models
{
    public class PartiallyTransformedBinaryOperatorExpression : IPartiallyTransformedBinaryOperatorExpression
    {
        public BinaryOperatorExpression BinaryOperatorExpression { get; set; }
        public IVhdlElement LeftTransformed { get; set; }
        public IVhdlElement RightTransformed { get; set; }
    }
}
