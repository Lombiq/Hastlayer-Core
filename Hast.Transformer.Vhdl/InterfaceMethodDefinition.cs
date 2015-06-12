using System.Collections.Generic;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl
{
    public class InterfaceMethodDefinition
    {
        public string Name { get; set; }
        public List<Port> Ports { get; set; }
        public Procedure Procedure { get; set; }
        public MethodDeclaration Method { get; set; }
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
