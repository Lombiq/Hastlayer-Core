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


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            var subContext = vhdlGenerationContext.CreateContextForSubLevel();

            return Terminated.Terminate(
                "procedure " + Name +
                (Parameters.Count > 0 ? " (" : " ") + vhdlGenerationContext.NewLineIfShouldFormat() +
                // Out params at the end.
                Parameters.OrderBy(parameter => parameter.ParameterType).ToVhdl(subContext, ";") +
                (Parameters.Count > 0 ? ")" : string.Empty)  +
                " is " + vhdlGenerationContext.NewLineIfShouldFormat() +
                Declarations.ToVhdl(subContext) + (Declarations != null && Declarations.Any() ? " " : string.Empty) +
                "begin " + vhdlGenerationContext.NewLineIfShouldFormat() +
                Body.ToVhdl(subContext) +
                " end procedure " + Name, vhdlGenerationContext);
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


        public override string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            return
                (DataObjectKind.ToString() ?? string.Empty) +
                Name +
                ": " +
                ParameterType +
                " " +
                DataType.ToVhdl(vhdlGenerationContext);
        }
    }
}
