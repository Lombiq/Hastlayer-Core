using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Process : ISubProgram
    {
        public string Label { get; set; }
        public string Name { get { return Label; } set { Label = value; } }
        public List<IDataObject> SesitivityList { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Process()
        {
            SesitivityList = new List<IDataObject>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                (!string.IsNullOrEmpty(Label) ? Label + ":" : string.Empty) +
                "process (" +
                string.Join("; ", SesitivityList.Select(signal => signal.Name)) +
                ") " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end process;";
        }
    }
}
