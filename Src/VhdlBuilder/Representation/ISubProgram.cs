using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public interface ISubProgram : IVhdlElement
    {
        string Name { get; set; }
        List<IVhdlElement> Declarations { get; set; }
        List<IVhdlElement> Body { get; set; }
    }
}
