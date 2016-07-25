using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public class BasicComponent : ArchitectureComponentBase
    {
        public IVhdlElement Declarations { get; set; }
        public IVhdlElement Body { get; set; }


        public BasicComponent(string name) : base(name)
        {
        }


        public override IVhdlElement BuildDeclarations()
        {
            return BuildDeclarationsBlock(Declarations);
        }

        public override IVhdlElement BuildBody()
        {
            if (Body == null) return Empty.Instance;

            return new LogicalBlock(
                new LineComment(Name + " start"),
                Body,
                new LineComment(Name + " end"));
        }
    }
}
