using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public interface INamedElement : IVhdlElement
    {
        string Name { get; set; }
    }
}
