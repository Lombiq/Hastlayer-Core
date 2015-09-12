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


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "type " + Name + " is record " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Members.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end record", vhdlGenerationOptions);
        }
    }
}
