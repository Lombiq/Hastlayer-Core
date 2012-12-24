using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using VhdlBuilder.Representation.Declaration;

namespace HastTranspiler.Vhdl.SubTranspilers
{
    public class MethodBodyContext
    {
        public TranspilingContext TranspilingContext { get; set; }
        public MethodBodyScope Scope { get; set; }
    }

    public class MethodBodyScope
    {
        public MethodDeclaration Method { get; set; }
        public Procedure Procedure { get; set; }
        public IBlockElement Block { get; set; }
    }
}
