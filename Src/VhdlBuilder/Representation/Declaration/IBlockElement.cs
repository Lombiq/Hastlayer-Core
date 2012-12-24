using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
{
    public interface IBlockElement : IVhdlElement
    {
        List<IVhdlElement> Body { get; set; }
    }
}
