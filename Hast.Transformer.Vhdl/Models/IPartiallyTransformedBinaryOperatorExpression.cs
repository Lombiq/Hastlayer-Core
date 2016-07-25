using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.Models
{
    public interface IPartiallyTransformedBinaryOperatorExpression
    {
        BinaryOperatorExpression BinaryOperatorExpression { get; }
        IVhdlElement LeftTransformed { get; }
        IVhdlElement RightTransformed { get; }
    }
}
