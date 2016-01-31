using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.Transformer.Vhdl.Constants;

namespace Hast.Transformer.Vhdl.StateMachineGeneration
{
    public class MemberStateMachine : IMemberStateMachine
    {
        private readonly Enum _statesEnum;
        private readonly Variable _stateVariable;
        private readonly Signal _startSignal;
        private readonly Signal _finishedSignal;

        public string Name { get; private set; }

        private List<IMemberStateMachineState> _states;
        public IReadOnlyList<IMemberStateMachineState> States
        {
            get { return _states; }
        }

        public IList<Variable> Parameters { get; private set; }
        public IList<Variable> LocalVariables { get; private set; }
        public IList<Signal> Signals { get; private set; }


        /// <summary>
        /// Constructs a new <see cref="MemberStateMachine"/> object.
        /// </summary>
        /// <param name="name">
        /// The name of the state machine, i.e. the name of the member. Use the real name, not the extended VHDL ID.
        /// </param>
        public MemberStateMachine(string name)
        {
            Name = name;

            Parameters = new List<Variable>();
            LocalVariables = new List<Variable>();
            Signals = new List<Signal>();


            _statesEnum = new Enum { Name = this.CreateNamePrefixedExtendedVhdlId("_States") };

            _stateVariable = new Variable
            {
                DataType = _statesEnum,
                Name = this.CreateStateVariableName()
            };
            LocalVariables.Add(_stateVariable);

            _startSignal = new Signal
            {
                DataType = KnownDataTypes.Boolean,
                Name = this.CreateStartSignalName(),
                InitialValue = Value.False
            };
            // The star signal is special since it's driven from the outside, so not adding it to Signals.

            _finishedSignal = new Signal
            {
                DataType = KnownDataTypes.Boolean,
                Name = this.CreateFinishedSignalName(),
                InitialValue = Value.True
            };


            var startStateBlock = new InlineBlock(
                new LineComment("Start state"),
                new Assignment { AssignTo = _finishedSignal, Expression = Value.False },
                new IfElse
                {
                    Condition = new Binary
                    {
                        Left = _startSignal.Name.ToVhdlSignalReference(), 
                        Operator = Operator.Equality, 
                        Right = Value.True
                    },
                    True = this.CreateStateChange(2)
                });

            var finalStateBlock = new InlineBlock(
                new LineComment("Final state"),
                new Assignment { AssignTo = _finishedSignal, Expression = Value.True },
                this.ChangeToStartState());

            _states = new List<IMemberStateMachineState>
            {
                new MemberStateMachineState { Body = startStateBlock },
                new MemberStateMachineState { Body = finalStateBlock }
            };
        }


        public int AddState(IBlockElement state)
        {
            _states.Add(new MemberStateMachineState { Body = state });
            return _states.Count - 1;
        }

        public IVhdlElement BuildDeclarations()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _statesEnum.Values.Add(this.CreateStateName(i).ToVhdlIdValue());
            }

            foreach (var parameter in Parameters)
            {
                parameter.Shared = true;
            }

            var declarationsBlock = new LogicalBlock(
                new LineComment(Name + " declarations start"),
                new LineComment("State machine states:"),
                _statesEnum,
                new LineComment("State machine control signals:"),
                _startSignal,
                _finishedSignal);

            if (Parameters.Any())
            {
                declarationsBlock.Add(new LineComment("Shared variables for the state machine's inputs and outputs:")); 
            }
            declarationsBlock.Body.AddRange(Parameters);

            if (Signals.Any())
            {
                declarationsBlock.Add(new LineComment("Global signals corresponding to the state machine:"));
            }
            declarationsBlock.Body.AddRange(Signals);

            declarationsBlock.Add(new LineComment(Name + " declarations end"));

            return declarationsBlock;
        }

        public IVhdlElement BuildBody()
        {
            var process = new Process { Name = this.CreateNamePrefixedExtendedVhdlId("_StateMachine") };

            process.Declarations = LocalVariables.Cast<IVhdlElement>().ToList();

            var stateCase = new Case { Expression = _stateVariable.Name.ToVhdlIdValue() };

            for (int i = 0; i < _states.Count; i++)
            {
                var stateWhen = new When { Expression = this.CreateStateName(i).ToVhdlIdValue() };
                stateWhen.Add(_states[i].Body);
                stateWhen.Add(new LineComment("Clock cycles needed to complete this state (approximation): " + _states[i].RequiredClockCycles));
                stateCase.Whens.Add(stateWhen);
            }

            var ifInResetBlock = new InlineBlock(
                new LineComment("Synchronous reset"),
                new Assignment { AssignTo = _finishedSignal, Expression = Value.False },
                this.CreateStateChange(0));
            ifInResetBlock.Body.AddRange(Signals.Select( signal =>
                new Assignment { AssignTo = signal.ToReference(), Expression = signal.InitialValue }));

            var resetIf = new IfElse
            {
                Condition = new Binary
                {
                    Left = CommonPortNames.Reset.ToVhdlSignalReference(),
                    Operator = Operator.Equality,
                    Right = Value.OneCharacter
                },
                True = ifInResetBlock,
                Else = stateCase
            };
            process.Add(resetIf);

            return new LogicalBlock(
                new LineComment(Name + " state machine start"), 
                process,
                new LineComment(Name + " state machine end"));
        }


        public class MemberStateMachineState : IMemberStateMachineState
        {
            public IBlockElement Body { get; set; }
            public decimal RequiredClockCycles { get; set; }
        }
    }
}
