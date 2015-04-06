using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public interface IBlockElement : IVhdlElement
    {
        List<IVhdlElement> Body { get; set; }
    }
}
