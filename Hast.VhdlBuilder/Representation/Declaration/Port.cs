using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public enum PortMode
    {
        In,
        Out,
        Buffer,
        InOut
    }


    public class Port : TypedDataObjectBase
    {
        public PortMode Mode { get; set; }


        public Port()
        {
            DataObjectKind = DataObjectKind.Signal;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                Name +
                ": " +
                Mode +
                " " +
                DataType.ToVhdl(vhdlGenerationOptions);
        }
    }
}
