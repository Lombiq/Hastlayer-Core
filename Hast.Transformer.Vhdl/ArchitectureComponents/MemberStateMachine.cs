using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Extensions;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.Transformer.Vhdl.Constants;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Transformer.Vhdl.ArchitectureComponents
{
    internal class MemberStateMachine : ArchitectureComponentBase, IMemberStateMachine
    {
        private readonly Enum _statesEnum;
        private readonly Variable _stateVariable;
        private readonly Signal _startedSignal;
        private readonly Signal _finishedSignal;

        private List<IMemberStateMachineState> _states;
        public IReadOnlyList<IMemberStateMachineState> States
        {
            get { return _states; }
        }


        public MemberStateMachine(string name) : base(name)
        {
            _statesEnum = new Enum { Name = this.CreatePrefixedObjectName("_States") };

            _stateVariable = new Variable
            {
                DataType = _statesEnum,
                Name = this.CreateStateVariableName()
            };
            LocalVariables.Add(_stateVariable);

            _startedSignal = new Signal
            {
                DataType = KnownDataTypes.Boolean,
                Name = this.CreateStartedSignalName(),
                InitialValue = Value.False
            };
            Signals.Add(_startedSignal);

            _finishedSignal = new Signal
            {
                DataType = KnownDataTypes.Boolean,
                Name = this.CreateFinishedSignalName(),
                InitialValue = Value.False
            };
            Signals.Add(_finishedSignal);


            var startStateBlock = new InlineBlock(
                new LineComment("Start state"),
                new LineComment("Waiting for the start signal."),
                new IfElse
                {
                    Condition = new Binary
                    {
                        Left = _startedSignal.Name.ToVhdlSignalReference(),
                        Operator = BinaryOperator.Equality,
                        Right = Value.True
                    },
                    True = this.CreateStateChange(2)
                });

            var finalStateBlock = new InlineBlock(
                new LineComment("Final state"),
                new LineComment("Signaling finished until Started is pulled back to false, then returning to the start state."),
                new IfElse
                {
                    Condition = new Binary
                    {
                        Left = _startedSignal.Name.ToVhdlSignalReference(),
                        Operator = BinaryOperator.Equality,
                        Right = Value.True
                    },
                    True = new Assignment { AssignTo = _finishedSignal, Expression = Value.True },
                    Else = new InlineBlock(
                        new Assignment { AssignTo = _finishedSignal, Expression = Value.False },
                        this.ChangeToStartState())
                });

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

        public override IVhdlElement BuildDeclarations()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _statesEnum.Values.Add(this.CreateStateName(i).ToVhdlIdValue());
            }

            return BuildDeclarationsBlock(new InlineBlock(
                new LineComment("State machine states:"),
                _statesEnum));
        }

        public override IVhdlElement BuildBody()
        {
            var stateCase = new Case { Expression = _stateVariable.Name.ToVhdlIdValue() };

            for (int i = 0; i < _states.Count; i++)
            {
                var stateWhen = new When { Expression = this.CreateStateName(i).ToVhdlIdValue() };
                stateWhen.Add(_states[i].Body);
                stateWhen.Add(new LineComment("Clock cycles needed to complete this state (approximation): " + _states[i].RequiredClockCycles));
                stateCase.Whens.Add(stateWhen);
            }

            var process = BuildProcess(stateCase, this.CreateStateChange(0));

            process.Name = this.CreatePrefixedObjectName("_StateMachine");

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
