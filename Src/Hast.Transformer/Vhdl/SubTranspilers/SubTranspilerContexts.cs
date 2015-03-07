using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.SubTranspilers
{
    public class SubTranspilerContext
    {
        public TranspilingContext TranspilingContext { get; set; }
        public SubTranspilerScope Scope { get; set; }
    }

    public class SubTranspilerScope
    {
        public AstNode Node { get; set; }
        public ISubProgram SubProgram { get; set; }
    }
}
