using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Helpers;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// A state machine generated from a .NET method.
    /// </summary>
    public interface IMethodStateMachine
    {
        string Name { get; }
        IEnumerable<IBlockElement> States { get; }
        IList<Variable> Parameters { get; }
        IList<Variable> LocalVariables { get; }


        /// <summary>
        /// Adds a new state to the state machine.
        /// </summary>
        /// <param name="state">The state's VHDL element.</param>
        /// <returns>The index of the state.</returns>
        int AddState(IBlockElement state);

        string CreateStateName(int index);
        IVhdlElement CreateStateChange(int nextStateIndex);

        /// <summary>
        /// Produces the declarations corresponding to the state machine that should be inserted into the head of the
        /// architecture element.
        /// </summary>
        IVhdlElement BuildDeclarations();

        /// <summary>
        /// Produces the body of the state machine that should be inserted into the body of the architecture element.
        /// </summary>
        IVhdlElement BuildBody();
    }


    public static class MethodStateMachineExtenions
    {
        public static IVhdlElement ChangeToStartState(this IMethodStateMachine stateMachine)
        {
            return stateMachine.CreateStateChange(0);
        }

        public static IVhdlElement ChangeToFinalState(this IMethodStateMachine stateMachine)
        {
            return stateMachine.CreateStateChange(1);
        }

        public static string CreateReturnVariableName(this IMethodStateMachine stateMachine)
        {
            return MethodStateMachineNameFactory.CreateReturnVariableName(stateMachine.Name);
        }

        public static string CreatePrefixedVariableName(this IMethodStateMachine stateMachine, string name)
        {
            return MethodStateMachineNameFactory.CreatePrefixedVariableName(stateMachine, name);
        }

        public static string CreateNamePrefixedExtendedVhdlId(this IMethodStateMachine stateMachine, string id)
        {
            return MethodStateMachineNameFactory.CreatePrefixedExtendedVhdlId(stateMachine.Name, id);
        }
    }
}
