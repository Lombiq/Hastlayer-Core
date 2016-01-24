using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public static class MemberStateMachineVariableHelper
    {
        /// <summary>
        /// Making sure that the e.g. return variable names are unique per method call (to transfer procedure outputs).
        /// </summary>
        public static string GetNextUnusedTemporaryVariableName(string name, IMemberStateMachine stateMachine)
        {
            var variableName = name + "0";
            var returnVariableNameIndex = 0;

            while (stateMachine.LocalVariables.Any(variable => 
                variable.Name == MemberStateMachineNameFactory.CreatePrefixedVariableName(stateMachine, variableName)))
            {
                variableName = name + ++returnVariableNameIndex;
            }

            return MemberStateMachineNameFactory.CreatePrefixedVariableName(stateMachine, variableName);
        }

        public static Variable CreateTemporaryVariable(
            string name,
            DataType dataType,
            IMemberStateMachine stateMachine)
        {
            var returnVariable = new Variable
            {
                Name = GetNextUnusedTemporaryVariableName(name, stateMachine),
                DataType = dataType
            };

            stateMachine.LocalVariables.Add(returnVariable);

            return returnVariable;
        }
    }
}
