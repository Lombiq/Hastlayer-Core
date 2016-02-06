using System.Collections.Generic;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlTransformationContext : TransformationContext, IVhdlTransformationContext
    {
        public VhdlTransformationContext(ITransformationContext previousContext) : base(previousContext)
        {
        }
    }
}
