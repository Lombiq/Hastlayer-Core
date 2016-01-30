using System.Collections.Generic;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlTransformationContext : TransformationContext, IVhdlTransformationContext
    {
        public IMemberStateMachineStartSignalFunnel MemberStateMachineStartSignalFunnel { get; set; }


        public VhdlTransformationContext(ITransformationContext previousContext) : base(previousContext)
        {
            MemberStateMachineStartSignalFunnel = new MemberStateMachineStartSignalFunnel();
        }
    }
}
