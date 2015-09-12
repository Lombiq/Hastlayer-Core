using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Enum : DataType
    {
        public List<Value> Values { get; set; }


        public Enum()
        {
            TypeCategory = DataTypeCategory.Composite;
            Values = new List<Value>();
        }


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return Terminated.Terminate(
                "type " + Name + " is (" + vhdlGenerationContext.NewLineIfShouldFormat() +
                    Values.ToVhdl(vhdlGenerationContext.CreateContextForSubLevel(), ",") +
                ")", vhdlGenerationContext);
        }
    }
}
