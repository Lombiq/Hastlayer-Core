﻿using System.Collections.Generic;
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
    internal class MemberStateMachine : IMemberStateMachine
    {
        private readonly Enum _statesEnum;
        private readonly Variable _stateVariable;
        private readonly Signal _startedSignal;
        private readonly Signal _finishedSignal;

        public string Name { get; private set; }

        private List<IMemberStateMachineState> _states;
        public IReadOnlyList<IMemberStateMachineState> States
        {
            get { return _states; }
        }

        public IList<Variable> LocalVariables { get; private set; }
        public IList<Variable> GlobalVariables { get; private set; }
        public IList<Signal> Signals { get; private set; }
        public IDictionary<string, int> OtherMemberMaxCallInstanceCounts { get; set; }


        /// <summary>
        /// Constructs a new <see cref="MemberStateMachine"/> object.
        /// </summary>
        /// <param name="name">
        /// The name of the state machine, i.e. the name of the member. Use the real name, not the extended VHDL ID.
        /// </param>
        public MemberStateMachine(string name)
        {
            Name = name;

            LocalVariables = new List<Variable>();
            GlobalVariables = new List<Variable>();
            Signals = new List<Signal>();
            OtherMemberMaxCallInstanceCounts = new Dictionary<string, int>();


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
                        Operator = Operator.Equality,
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
                        Operator = Operator.Equality,
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

        public IVhdlElement BuildDeclarations()
        {
            for (int i = 0; i < _states.Count; i++)
            {
                _statesEnum.Values.Add(this.CreateStateName(i).ToVhdlIdValue());
            }

            foreach (var variable in GlobalVariables)
            {
                variable.Shared = true;
            }

            var declarationsBlock = new LogicalBlock(
                new LineComment(Name + " declarations start"),
                new LineComment("State machine states:"),
                _statesEnum);

            if (Signals.Any())
            {
                declarationsBlock.Add(new LineComment("Signals of the state machine, starting with control signals:"));
            }
            declarationsBlock.Body.AddRange(Signals);

            if (GlobalVariables.Any())
            {
                declarationsBlock.Add(new LineComment("Shared variables of the state machine (mainly for inputs and outputs):"));
            }
            declarationsBlock.Body.AddRange(GlobalVariables);

            declarationsBlock.Add(new LineComment(Name + " declarations end"));

            return declarationsBlock;
        }

        public IVhdlElement BuildBody()
        {
            var process = new Process { Name = this.CreatePrefixedObjectName("_StateMachine") };

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
                this.CreateStateChange(0));

            // Re-setting all signals and variables to their initial value.
            ifInResetBlock.Body.AddRange(Signals
                .Where(signal => signal.InitialValue != null)
                .Select(signal =>
                    new Assignment { AssignTo = signal.ToReference(), Expression = signal.InitialValue }));
            ifInResetBlock.Body.AddRange(LocalVariables
                .Union(GlobalVariables)
                .Where(variable => variable.InitialValue != null)
                .Select(variable =>
                    new Assignment { AssignTo = variable.ToReference(), Expression = variable.InitialValue }));

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
