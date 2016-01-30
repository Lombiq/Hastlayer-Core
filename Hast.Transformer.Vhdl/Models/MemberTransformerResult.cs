using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.StateMachineGeneration;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    internal class MemberTransformerResult : IMemberTransformerResult
    {
        public IEnumerable<IMemberStateMachineResult> StateMachines { get; set; }
    }
}
