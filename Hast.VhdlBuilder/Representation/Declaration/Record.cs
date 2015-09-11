using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Record : DataType
    {
        public List<TypedDataObjectBase> Members { get; set; }


        public Record()
        {
            Members = new List<TypedDataObjectBase>();
        }


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                "type " +
                Name +
                " is record " +
                Members.ToVhdl() +
                " end record;";
        }
    }
}
