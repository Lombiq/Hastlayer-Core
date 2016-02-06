using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public interface IMemberStateMachineFactory : IDependency
    {
        IMemberStateMachine CreateStateMachine(string name);
    }


    public class MemberStateMachineFactory : IMemberStateMachineFactory
    {

        public IMemberStateMachine CreateStateMachine(string name)
        {
            return new MemberStateMachine(name);
        }
    }
}
