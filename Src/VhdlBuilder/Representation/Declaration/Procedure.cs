using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
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
                " (" +
                string.Join("; ", Parameters.Select(parameter => parameter.ToVhdl())) +
                ") is " +
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
        Out,
        InOut
    }

    public class ProcedureParameter : IVhdlElement
    {
        public ObjectType ObjectType { get; set; }
        public string Name { get; set; }
        public ProcedureParameterType ParameterType { get; set; }
        public DataType DataType { get; set; }

        public string ToVhdl()
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
