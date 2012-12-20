using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    /// <summary>
    /// Represents an entity in a VHDL source
    /// </summary>
    /// <remarks>
    /// It's basically anything in VHDL.
    /// </remarks>
    public interface IVhdlElement
    {
        string ToVhdl();
    }
}
