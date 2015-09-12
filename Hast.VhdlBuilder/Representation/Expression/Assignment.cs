using System.Diagnostics;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Representation.Expression
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Assignment : IVhdlElement
    {
        public IDataObject AssignTo { get; set; }
        public IVhdlElement Expression { get; set; }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                AssignTo.Name +
                (AssignTo.DataObjectKind == DataObjectKind.Variable ? " := " : " <= ") +
                Expression.ToVhdl(vhdlGenerationContext);
        }
    }
}
