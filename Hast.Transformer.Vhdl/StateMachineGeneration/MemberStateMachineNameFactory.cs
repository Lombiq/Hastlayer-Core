using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Extensions;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public static class MemberStateMachineNameFactory
    {
        public static string CreateReturnVariableName(string stateMachineName)
        {
            return CreatePrefixedVariableName(stateMachineName, "return");
        }

        public static string CreateStartVariableName(string stateMachineName)
        {
            return CreatePrefixedVariableName(stateMachineName, "_Start");
        }

        public static string CreateFinishedVariableName(string stateMachineName)
        {
            return CreatePrefixedVariableName(stateMachineName, "_Finished");
        }

        public static string CreateStateVariableName(string stateMachineName)
        {
            return CreatePrefixedVariableName(stateMachineName, "_State");
        }

        public static string CreatePrefixedVariableName(IMemberStateMachine stateMachine, string name)
        {
            return CreatePrefixedVariableName(stateMachine.Name, name);
        }

        public static string CreatePrefixedVariableName(string stateMachineName, string name)
        {
            return CreatePrefixedExtendedVhdlId(stateMachineName, "." + name);
        }

        public static string CreatePrefixedExtendedVhdlId(string stateMachineName, string id)
        {
            return (stateMachineName + id).ToExtendedVhdlId();
        }

        public static string CreateStateMachineName(string stateMachineName, int stateMachineIndex)
        {
            return stateMachineName + "." + stateMachineIndex;
        }
    }
}
