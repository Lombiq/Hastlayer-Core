﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public static class MemberStateMachineExtenions
    {
        public static IVhdlElement ChangeToStartState(this IMemberStateMachine stateMachine)
        {
            return stateMachine.CreateStateChange(0);
        }

        public static IVhdlElement ChangeToFinalState(this IMemberStateMachine stateMachine)
        {
            return stateMachine.CreateStateChange(1);
        }


        public static string CreateStartVariableName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateStartVariableName(stateMachine.Name);
        }

        public static string CreateFinishedVariableName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateFinishedVariableName(stateMachine.Name);
        }

        public static string CreateReturnVariableName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateReturnVariableName(stateMachine.Name);
        }

        public static string CreateStateVariableName(this IMemberStateMachine stateMachine)
        {
            return MemberStateMachineNameFactory.CreateStateVariableName(stateMachine.Name);
        }

        public static string CreatePrefixedVariableName(this IMemberStateMachine stateMachine, string name)
        {
            return MemberStateMachineNameFactory.CreatePrefixedVariableName(stateMachine.Name, name);
        }

        public static string CreateNamePrefixedExtendedVhdlId(this IMemberStateMachine stateMachine, string id)
        {
            return MemberStateMachineNameFactory.CreatePrefixedExtendedVhdlId(stateMachine.Name, id);
        }
    }
}