using System.Collections.Generic;
using Hast.Transformer.Models;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public interface IVhdlTransformationContext : ITransformationContext
    {
        IMemberStateMachineStartedSignalFunnel MemberStateMachineStartSignalFunnel { get; }
    }
}
