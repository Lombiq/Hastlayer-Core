using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponentBuilding;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    internal class MemberStateMachineResult : IMemberStateMachineResult
    {
        public IMemberStateMachine StateMachine { get; set; }
        public IVhdlElement Declarations { get; set; }
        public IVhdlElement Body { get; set; }
    }
}
