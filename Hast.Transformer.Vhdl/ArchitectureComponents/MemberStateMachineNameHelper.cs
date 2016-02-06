using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    public static class MemberStateMachineNameHelper
    {
        public static string CreateStateMachineName(string stateMachineName, int stateMachineIndex)
        {
            return stateMachineName + "." + stateMachineIndex;
        }
    }
}
