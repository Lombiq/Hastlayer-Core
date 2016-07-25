using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Alias : TypedDataObjectBase
    {
        /// <summary>
        /// Name of the object that the alias is created for.
        /// </summary>
        public string ObjectName { get; set; }


        public Alias()
        {
            DataObjectKind = DataObjectKind.Variable;
        }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "alias " +
                vhdlGenerationOptions.ShortenName(Name) +
                " : " +
                DataType.ToReference().ToVhdl(vhdlGenerationOptions) +
                " is " +
                vhdlGenerationOptions.ShortenName(ObjectName),
                vhdlGenerationOptions);
        }
    }
}
