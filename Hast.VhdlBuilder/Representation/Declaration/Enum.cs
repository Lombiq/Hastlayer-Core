using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Representation.Expression;

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
            var subContext = vhdlGenerationContext.CreateContextForSubLevel();

            var vhdl =
                "type " + Name + " is (";

            foreach (var value in Values)
            {
                vhdl += subContext.IndentIfShouldFormat() + value.ToVhdl(subContext) + subContext.NewLineIfShouldFormat();
            }
            vhdl += ")";

            return Terminated.Terminate(vhdl, vhdlGenerationContext);
        }
    }
}
