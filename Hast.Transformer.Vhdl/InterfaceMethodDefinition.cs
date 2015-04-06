using System.Collections.Generic;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl
{
    public class InterfaceMethodDefinition
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }
        public Procedure Procedure { get; set; }
        public List<ParameterMapping> ParameterMappings { get; set; }


        public InterfaceMethodDefinition()
        {
            Ports = new List<Port>();
            ParameterMappings = new List<ParameterMapping>();
        }
    }


    public class ParameterMapping
    {
        public ProcedureParameter Parameter { get; set; }
        public Port Port { get; set; }
    }
}
