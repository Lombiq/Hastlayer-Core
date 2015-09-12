using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Function : ISubProgram
    {
        public string Name { get; set; }
        public List<FunctionArgument> Arguments { get; set; }
        public DataType ReturnType { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Function()
        {
            Arguments = new List<FunctionArgument>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            var name = vhdlGenerationOptions.ShortenName(Name);
            return Terminated.Terminate(
                "function " + name +
                " (" + Arguments.ToVhdl(vhdlGenerationOptions, "; ", string.Empty) + ") " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                "return " + ReturnType.Name + " is " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Declarations.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                    (Declarations != null && Declarations.Any() ? " " : string.Empty) +
                "begin " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Body.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end " + name, vhdlGenerationOptions);
        }
    }


    public class FunctionArgument : TypedDataObjectBase
    {
        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                (DataObjectKind.ToString() ?? string.Empty) +
                vhdlGenerationOptions.ShortenName(Name) +
                ": " +
                DataType.ToVhdl(vhdlGenerationOptions);
        }
    }
}
