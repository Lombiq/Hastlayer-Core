using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// Represents a block of VHDL elements that logically correspond to each other. The block has only a
    /// semantic difference to <see cref="InlineBlock"/>.
    /// </summary>
    public class LogicalBlock : InlineBlock
    {
        public LogicalBlock(params IVhdlElement[] vhdlElements) : base(vhdlElements)
        {
        }

        public LogicalBlock() : base()
        {
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                vhdlGenerationOptions.NewLineIfShouldFormat() +
                base.ToVhdl(vhdlGenerationOptions) +
                vhdlGenerationOptions.NewLineIfShouldFormat();
        }
    }
}
