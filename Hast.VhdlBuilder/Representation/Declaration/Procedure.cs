using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Procedure : ISubProgram
    {
        public string Name { get; set; }
        public List<ProcedureParameter> Parameters { get; set; }
        public List<IVhdlElement> Declarations { get; set; }
        public List<IVhdlElement> Body { get; set; }


        public Procedure()
        {
            Parameters = new List<ProcedureParameter>();
            Declarations = new List<IVhdlElement>();
            Body = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return Terminated.Terminate(
                "procedure " + Name +
                (Parameters.Count > 0 ? " (" : " ") + vhdlGenerationOptions.NewLineIfShouldFormat() +
                // Out params at the end.
                Parameters.OrderBy(parameter => parameter.ParameterType).ToVhdl(vhdlGenerationOptions, ";") +
                (Parameters.Count > 0 ? ")" : string.Empty)  +
                " is " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Declarations.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) + (Declarations != null && Declarations.Any() ? " " : string.Empty) +
                "begin " + vhdlGenerationOptions.NewLineIfShouldFormat() +
                    Body.ToVhdl(vhdlGenerationOptions).IndentLinesIfShouldFormat(vhdlGenerationOptions) +
                "end procedure " + Name, vhdlGenerationOptions);
        }
    }


    public enum ProcedureParameterType
    {
        In,
        InOut,
        Out
    }


    [DebuggerDisplay("{ToVhdl()}")]
    public class ProcedureParameter : TypedDataObjectBase
    {
        public ProcedureParameterType ParameterType { get; set; }


        public override string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                (DataObjectKind.ToString() ?? string.Empty) +
                Name +
                ": " +
                ParameterType +
                " " +
                DataType.ToVhdl(vhdlGenerationOptions);
        }
    }
}
