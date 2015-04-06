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
    public class Process : ISubProgram
    {
        public string Name { get; set; }
        public List<IDataObject> SesitivityList { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Process()
        {
            SesitivityList = new List<IDataObject>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl()
        {
            var vhdl = "";

            if (!string.IsNullOrEmpty(Name)) vhdl += Name.ToExtendedVhdlId() + ": ";

            return
                "process (" +
                string.Join("; ", SesitivityList.Select(signal => signal.Name.ToExtendedVhdlId())) +
                ") " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end process;";
        }
    }
}
