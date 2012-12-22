using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation
{
    public class Procedure : IVhdlElement
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
            var builder = new StringBuilder(11);

            if (!String.IsNullOrEmpty(Name))
            {
                builder
                    .Append(Name.ToVhdlId())
                    .Append(": ");
            }

            var parametersVhdl = Parameters.ToVhdl();

            builder
                .Append("procedure ")
                .Append(Name.ToVhdlId())
                .Append(" (")
                .Append(parametersVhdl.Substring(0, parametersVhdl.Length - 1)) // Cutting off trailing semicolon
                .Append(") is ")
                .Append(Declarations.ToVhdl())
                .Append(" begin ")
                .Append(Body.ToVhdl())
                .Append(" end ")
                .Append(Name.ToVhdlId())
                .Append(";");

            return builder.ToString();
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
        public string Name { get; set; }
        public ProcedureParameterType ParameterType { get; set; }
        public DataType DataType { get; set; }

        public string ToVhdl()
        {
            var builder = new StringBuilder();
            builder
                .Append(Name.ToVhdlId())
                .Append(": ")
                .Append(ParameterType)
                .Append(" ")
                .Append(DataType.ToVhdl())
                .Append(";");

            return builder.ToString();
        }
    }
}
