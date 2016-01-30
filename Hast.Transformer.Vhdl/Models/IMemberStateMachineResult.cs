using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.StateMachineGeneration;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    public interface IMemberStateMachineResult
    {
        IMemberStateMachine StateMachine { get; }
        public IVhdlElement Declarations { get; }
        public IVhdlElement Body { get; }
    }
}
