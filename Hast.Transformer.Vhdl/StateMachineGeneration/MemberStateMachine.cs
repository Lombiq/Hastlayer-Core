using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Helpers;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public class MemberStateMachine : IMemberStateMachine
    {
        private readonly Enum _statesEnum;
        private readonly Variable _stateVariable;
        private readonly Variable _startVariable;
        private readonly Variable _finishedVariable;

        public string Name { get; private set; }

        private List<IBlockElement> _states;
        public IReadOnlyList<IBlockElement> States
        {
            get { return _states; }
        }

        public IList<Variable> Parameters { get; private set; }
        public IList<Variable> LocalVariables { get; private set; }


        /// <summary>
        /// Constructs a new <see cref="MemberStateMachine"/> object.
        /// </summary>
        /// <param name="name">
        /// The name of the state machine, i.e. the name of the member. Use the real name, not the extended VHDL ID.
        /// </param>
        public MemberStateMachine(string name)
        {
            Name = name;


            _statesEnum = new Enum { Name = this.CreateNamePrefixedExtendedVhdlId("_States") };

            _stateVariable = new Variable
            {
                DataType = _statesEnum,
                Name = this.CreateNamePrefixedExtendedVhdlId("_State"),
                Shared = true
            };

            _startVariable = new Variable
            {
                DataType = KnownDataTypes.Boolean,
                Name = MemberStateMachineNameFactory.CreateStartVariableName(Name),
                Shared = true,
                DefaultValue = Value.False
            };

            _finishedVariable = new Variable
            {
                DataType = KnownDataTypes.Boolean,
                Name = MemberStateMachineNameFactory.CreateFinishedVariableName(Name),
                Shared = true,
                DefaultValue = Value.True
            };


            var startStateBlock = new InlineBlock(
                new Comment("Start state"),
                new Assignment { AssignTo = _finishedVariable, Expression = Value.False }.Terminate(),
                new IfElse
                {
                    Condition = new Binary { Left = _startVariable.Name.ToVhdlIdValue(), Operator = Operator.Equality, Right = Value.True },
                    True = CreateStateChange(2)
                });

            var finalStateBlock = new InlineBlock(
                new Comment("Final state"),
                new Assignment { AssignTo = _finishedVariable, Expression = Value.True }.Terminate(),
                new Assignment { AssignTo = _startVariable, Expression = Value.False }.Terminate(),
                this.ChangeToStartState());

            _states = new List<IBlockElement>
            {
                startStateBlock,
                finalStateBlock
            };


            Parameters = new List<Variable>();
            LocalVariables = new List<Variable>();
        }


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

            var declarationsBlock = new LogicalBlock(
                new Comment(Name + " declarations start"),
                new Comment("State machine states"),
                _statesEnum,
                new Comment("State machine control variables"),
                _stateVariable,
                _startVariable,
                _finishedVariable);

            if (Parameters.Any())
            {
                declarationsBlock.Add(new Comment("Shared variables for the state machine's inputs and outputs")); 
            }
            declarationsBlock.Body.AddRange(Parameters);

            declarationsBlock.Add(new Comment(Name + " declarations end"));

            return declarationsBlock;
        }

        public IVhdlElement BuildBody()
        {
            var process = new Process { Name = this.CreateNamePrefixedExtendedVhdlId("_StateMachine") };

            process.Declarations = LocalVariables.Cast<IVhdlElement>().ToList();

            var stateCase = new Case { Expression = _stateVariable.Name.ToVhdlIdValue() };

            for (int i = 0; i < _states.Count; i++)
            {
                var stateWhen = new When { Expression = CreateStateName(i).ToVhdlIdValue() };
                stateWhen.Add(_states[i]);
                stateCase.Whens.Add(stateWhen);
            }

            var ifInResetBlock = new InlineBlock(
                new Comment("Synchronous reset"),
                new Assignment { AssignTo = _startVariable, Expression = Value.False }.Terminate(),
                new Assignment { AssignTo = _finishedVariable, Expression = Value.False }.Terminate(),
                CreateStateChange(0));

            var resetIf = new IfElse
            {
                Condition = new Binary
                {
                    Left = CommonPortNames.Reset.ToVhdlIdValue(),
                    Operator = Operator.Equality,
                    Right = new Character('1')
                },
                True = ifInResetBlock,
                Else = stateCase
            };
            process.Add(resetIf);

            return new LogicalBlock(
                new Comment(Name + " state machine start"), 
                process,
                new Comment(Name + " state machine end"));
        }


        private Value CreateStateValue(int index)
        {
            return CreateStateName(index).ToVhdlIdValue();
        }
    }
}
