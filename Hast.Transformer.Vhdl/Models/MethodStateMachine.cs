using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.Transformer.Vhdl.Constants;

namespace Hast.Transformer.Vhdl.Models
{
    /// <summary>
    /// A state machine generated from a .NET method.
    /// </summary>
    public class MethodStateMachine
    {
        private readonly Enum _statesEnum;
        private readonly Variable _stateVariable;
        private readonly Variable _startVariable;
        private readonly Variable _finishedVariable;

        public string Name { get; private set; }

        private List<IBlockElement> _states;
        /// <summary>
        /// States of the state machine. The state with the index 0 is the start state, the one with the index 1 is the
        /// final state.
        /// </summary>
        public IEnumerable<IBlockElement> States
        {
            get { return _states; }
        }

        public IList<Variable> Parameters { get; set; }
        public IList<Variable> LocalVariables { get; set; }


        /// <summary>
        /// Constructs a new <see cref="MethodStateMachine"/> object.
        /// </summary>
        /// <param name="name">
        /// The name of the state machine, i.e. the name of the method. Use the real name, not the extended VHDL ID.
        /// </param>
        public MethodStateMachine(string name)
        {
            Name = name;


            _statesEnum = new Enum { Name = CreateNamePrefixedExtendedVhdlId("_States") };

            _stateVariable = new Variable
            {
                DataType = _statesEnum,
                Name = CreateNamePrefixedExtendedVhdlId("_State"),
                Shared = true
            };

            _startVariable = new Variable
            {
                DataType = KnownDataTypes.Boolean,
                Name = CreateStartVariableName(Name),
                Shared = true,
                DefaultValue = Value.False
            };

            _finishedVariable = new Variable
            {
                DataType = KnownDataTypes.Boolean,
                Name = CreateFinishedVariableName(Name),
                Shared = true,
                DefaultValue = Value.True
            };


            var startStateBlock = new InlineBlock();
            startStateBlock.Add(new Assignment { AssignTo = _finishedVariable, Expression = Value.False }.Terminate());
            var startStateIfElse = new IfElse
            {
                Condition = new Binary { Left = _startVariable.Name.ToVhdlIdValue(), Operator = "=", Right = Value.True },
                True = CreateStateChange(2)
            };
            startStateBlock.Add(startStateIfElse);

            var finalStateBlock = new InlineBlock();
            finalStateBlock.Add(new Assignment { AssignTo = _finishedVariable, Expression = Value.True }.Terminate());
            finalStateBlock.Add(new Assignment { AssignTo = _startVariable, Expression = Value.False }.Terminate());
            finalStateBlock.Add(ChangeToStartState());

            _states = new List<IBlockElement>
            {
                startStateBlock,
                finalStateBlock
            };


            Parameters = new List<Variable>();
            LocalVariables = new List<Variable>();
        }


        /// <summary>
        /// Adds a new state to the state machine.
        /// </summary>
        /// <param name="state">The state's VHDL element.</param>
        /// <returns>The index of the state.</returns>
        public int AddState(IBlockElement state)
        {
            _states.Add(state);
            return _states.Count - 1;
        }

        public string CreateStateName(int index)
        {
            return (Name + "_State_" + index).ToExtendedVhdlId();
        }

        public IVhdlElement CreateStateChange(int nextStateIndex)
        {
            return new Assignment { AssignTo = _stateVariable, Expression = CreateStateValue(nextStateIndex) }.Terminate();
        }

        public IVhdlElement ChangeToStartState()
        {
            return CreateStateChange(0);
        }

        public IVhdlElement ChangeToFinalState()
        {
            return CreateStateChange(1);
        }

        public string CreateReturnVariableName()
        {
            return CreateReturnVariableName(Name);
        }

        public string CreatePrefixedVariableName(string name)
        {
            return CreatePrefixedVariableName(this, name);
        }

        public string CreateNamePrefixedExtendedVhdlId(string id)
        {
            return CreatePrefixedExtendedVhdlId(Name, id);
        }

        /// <summary>
        /// Produces the declarations corresponding to the state machine that should be inserted into the head of the
        /// architecture element.
        /// </summary>
        public IVhdlElement BuildDeclarations()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _statesEnum.Values.Add(CreateStateValue(i));
            }

            foreach (var parameter in Parameters)
            {
                parameter.Shared = true;
            }

            var declarationsBlock = new InlineBlock();

            declarationsBlock.Body.AddRange(new IVhdlElement[]
            {
                _statesEnum,
                _stateVariable,
                _startVariable,
                _finishedVariable
            });
            declarationsBlock.Body.AddRange(Parameters);

            return declarationsBlock;
        }

        /// <summary>
        /// Produces the body of the state machine that should be inserted into the body of the architecture element.
        /// </summary>
        public IVhdlElement BuildBody()
        {
            var process = new Process { Name = CreateNamePrefixedExtendedVhdlId("_StateMachine") };

            process.Declarations = LocalVariables.Cast<IVhdlElement>().ToList();

            var stateCase = new Case { Expression = _stateVariable.Name.ToVhdlIdValue() };

            for (int i = 0; i < _states.Count; i++)
            {
                var stateWhen = new When { Expression = CreateStateName(i).ToVhdlIdValue() };
                stateWhen.Add(_states[i]);
                stateCase.Whens.Add(stateWhen);
            }

            var ifInResetBlock = new InlineBlock(
                new Assignment { AssignTo = _startVariable, Expression = Value.False }.Terminate(),
                new Assignment { AssignTo = _finishedVariable, Expression = Value.False }.Terminate(),
                CreateStateChange(0));

            var resetIf = new IfElse
            {
                Condition = new Binary
                {
                    Left = CommonPortNames.Reset.ToVhdlIdValue(),
                    Operator = "=",
                    Right = new Character('1')
                },
                True = ifInResetBlock,
                Else = stateCase
            };
            process.Add(resetIf);

            return process;
        }


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

        public static string CreatePrefixedVariableName(MethodStateMachine stateMachine, string name)
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

        public static string CreateStateMachineName(string stateMachineName, int index)
        {
            return stateMachineName + "." + index;
        }


        private Value CreateStateValue(int index)
        {
            return CreateStateName(index).ToVhdlIdValue();
        }
    }
}
