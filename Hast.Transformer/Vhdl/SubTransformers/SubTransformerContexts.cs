using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class SubTransformerContext
    {
        public TransformingContext TransformingContext { get; set; }
        public SubTransformerScope Scope { get; set; }
    }


    public class SubTransformerScope
    {
        public AstNode Node { get; set; }
        public ISubProgram SubProgram { get; set; }
    }
}
