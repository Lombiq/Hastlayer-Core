using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation.Declaration;

namespace HastTranspiler.Vhdl.SubTranspilers
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
