using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// An element that has a body but is inline, i.e. not surrounded by anything
    /// </summary>
    public class InlineBlock : IBlockElement
    {
        public List<IVhdlElement> Body { get; set; }


        public InlineBlock()
        {
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            return Body.ToVhdl();
        }
    }
}
