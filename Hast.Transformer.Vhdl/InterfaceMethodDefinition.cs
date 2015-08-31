using System.Collections.Generic;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl
{
    public class InterfaceMethodDefinition
    {
        public string Name { get; set; }
        public MethodStateMachine StateMachine { get; set; }
        public MethodDeclaration Method { get; set; }
    }
}
