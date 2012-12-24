using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder.Representation.Declaration
{
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


        public string ToVhdl()
        {
            return
                "function " +
                Name.ToVhdlId() +
                " (" +
                string.Join("; ", Arguments.Select(parameter => parameter.ToVhdl())) +
                ") return " +
                ReturnType.Name +
                " is " +
                Declarations.ToVhdl() +
                " begin " +
                Body.ToVhdl() +
                " end " +
                Name.ToVhdlId() +
                ";";
        }
    }


    public class FunctionArgument : DataObjectBase
    {
        public override string ToVhdl()
        {
            return
                (ObjectType.ToString() ?? string.Empty) +
                Name.ToVhdlId() +
                ": " +
                DataType.ToVhdl();
        }
    }
}
