using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
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
                Name.ToVhdlId() +
                (Parameters.Count > 0 ? " (" : " ") +
                // Out params at the end

                string.Join("; ", Parameters.OrderBy(parameter => parameter.ParameterType).Select(parameter => parameter.ToVhdl())) +
                (Parameters.Count > 0 ? ")" : string.Empty)  +
                " is " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end procedure " +
                Name.ToVhdlId() +
                ";";
        }
    }

    public enum ProcedureParameterType
    {
        In,
        InOut,
        Out
    }

    public class ProcedureParameter : DataObjectBase
    {
        public ProcedureParameterType ParameterType { get; set; }

        public override string ToVhdl()
        {
            return
                (ObjectType.ToString() ?? string.Empty) +
                Name.ToVhdlId() +
                ": " +
                ParameterType +
                " " +
                DataType.ToVhdl();
        }
    }
}
