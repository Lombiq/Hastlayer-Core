using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.Models
{
    public class PartiallyTransformedBinaryOperatorExpression : IPartiallyTransformedBinaryOperatorExpression
    {
        public BinaryOperatorExpression BinaryOperatorExpression { get; set; }
        public IVhdlElement LeftTransformed { get; set; }
        public IVhdlElement RightTransformed { get; set; }
    }
}
