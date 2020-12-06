using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public interface IDeclarableElement : IVhdlElement
    {
        List<IVhdlElement> Declarations { get; }
    }
}
