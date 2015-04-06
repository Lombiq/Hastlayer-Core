using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Extensions;
using System.Diagnostics;

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


        public string ToVhdl()
        {
            return
                "procedure " +
                Name.ToExtendedVhdlId() +
                (Parameters.Count > 0 ? " (" : " ") +
                // Out params at the end

                string.Join("; ", Parameters.OrderBy(parameter => parameter.ParameterType).Select(parameter => parameter.ToVhdl())) +
                (Parameters.Count > 0 ? ")" : string.Empty)  +
                " is " +
                Declarations.ToVhdl() + (Declarations != null && Declarations.Any() ? " " : string.Empty) +
                "begin " +
                Body.ToVhdl() +
                " end procedure " +
                Name.ToExtendedVhdlId() +
                ";";
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


        public override string ToVhdl()
        {
            return
                (DataObjectKind.ToString() ?? string.Empty) +
                Name.ToExtendedVhdlId() +
                ": " +
                ParameterType +
                " " +
                DataType.ToVhdl();
        }
    }
}
