using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Module : IVhdlElement
    {
        public List<Library> Libraries { get; set; }
        public Entity Entity { get; set; }
        public Architecture Architecture { get; set; }


        public Module()
        {
            Libraries = new List<Library>();
        }


        public string ToVhdl()
        {
            return Libraries.ToVhdl() + Entity.ToVhdl() + Architecture.ToVhdl();
        }
    }
}
