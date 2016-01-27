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
            return CreatePrefixedObjectName(stateMachineName, "return");
        }

        public static string CreateStartSignalName(string stateMachineName)
        {
            return CreatePrefixedObjectName(stateMachineName, "_Start");
        }

        public static string CreateFinishedSignalName(string stateMachineName)
        {
            return CreatePrefixedObjectName(stateMachineName, "_Finished");
        }

        public static string CreateStateVariableName(string stateMachineName)
        {
            return CreatePrefixedObjectName(stateMachineName, "_State");
        }

        public static string CreatePrefixedObjectName(string stateMachineName, string name)
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
