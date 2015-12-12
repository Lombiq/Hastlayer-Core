using System.Collections.Generic;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlTransformationContext : TransformationContext, IVhdlTransformationContext
    {
        public Module Module { get; set; }
        public IList<InterfaceMethodDefinition> InterfaceMethods { get; set; }
        public MemberCallChainTable MemberCallChainTable { get; set; }


        public VhdlTransformationContext(ITransformationContext previousContext) : base(previousContext)
        {
            InterfaceMethods = new List<InterfaceMethodDefinition>();
        }
    }
}
