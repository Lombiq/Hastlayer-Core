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


            _statesEnum = new Enum { Name = (Name + "_States").ToExtendedVhdlId() };

            _stateVariable = new Variable
            {
                DataType = new DataType { TypeCategory = DataTypeCategory.Composite, Name = _statesEnum.Name },
                Name = (Name + "_State").ToExtendedVhdlId(),
                Shared = true
            };

            _startVariable = new Variable
            {
                DataType = KnownDataTypes.Boolean,
                Name = (Name + "_Start").ToExtendedVhdlId(),
                Shared = true,
                DefaultValue = Value.False
            };

            _finishedVariable = new Variable
            {
                DataType = KnownDataTypes.Boolean,
                Name = (Name + "_Finished").ToExtendedVhdlId(),
                Shared = true,
                DefaultValue = Value.True
            };


            var startStateBlock = new InlineBlock();
            startStateBlock.Body.Add(new Terminated(new Assignment { AssignTo = _finishedVariable, Expression = Value.False }));
            var startStateIfElse = new IfElse
            {
                Condition = new Binary { Left = _startVariable.Name.ToVhdlIdValue(), Operator = "=", Right = Value.True },
                True = CreateStateChange(2)
            };
            startStateBlock.Body.Add(startStateIfElse);

            var finalStateBlock = new InlineBlock();
            finalStateBlock.Body.Add(new Terminated(new Assignment { AssignTo = _finishedVariable, Expression = Value.True }));
            finalStateBlock.Body.Add(ChangeToStartState());

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
            return new Terminated(new Assignment { AssignTo = _stateVariable, Expression = CreateStateValue(nextStateIndex) });
        }

        public IVhdlElement ChangeToStartState()
        {
            return CreateStateChange(0);
        }

        public IVhdlElement ChangeToFinalState()
        {
            return CreateStateChange(1);
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

            declarationsBlock.Body.AddRange(new IVhdlElement[] { _statesEnum, _stateVariable });
            declarationsBlock.Body.AddRange(Parameters);

            return declarationsBlock;
        }

        /// <summary>
        /// Produces the body of the state machine that should be inserted into the body of the architecture element.
        /// </summary>
        public IVhdlElement BuildBody()
        {
            var process = new Process { Name = (Name + "_StateMachine").ToExtendedVhdlId() };

            process.Declarations = LocalVariables.Cast<IVhdlElement>().ToList();

            var stateCase = new Case { Expression = _stateVariable.Name.ToVhdlIdValue() };

            for (int i = 0; i < _states.Count; i++)
            {
                var stateWhen = new When { Expression = CreateStateName(i).ToVhdlIdValue() };
                stateWhen.Body.Add(_states[i]);
                stateCase.Whens.Add(stateWhen);
            }

            var resetIf = new IfElse
            {
                Condition = new Binary
                {
                    Left = CommonPortNames.Reset.ToVhdlIdValue(),
                    Operator = "=",
                    Right = new Character('1')
                },
                True = CreateStateChange(0),
                Else = stateCase
            };
            process.Body.Add(resetIf);

            return process;
            /*
                STM_Primecalculator_0: process (\Clock\)
    begin
        if (rising_edge(\Clock\)) then
            if \Reset\ = '1' then
                state_SM_Primecalculator_0 <= ST000_Primecalculator_0;
            else
                case (state_SM_Primecalculator_0) is
                    
                    when ST000_Primecalculator_0 =>
                        Primecalc_Finished_0 <= '0';
                        if StartPrimeCalculator_0 = '1' then
                            state_SM_Primecalculator_0 <= ST001_Primecalculator_0;
                        end if; 
                        
                    when ST001_Primecalculator_0 =>
                        \PrimeCalcDataIn_0\ := \number.param\;
                        state_SM_Primecalculator_0 <= ST002_Primecalculator_0;
                        
                    when ST002_Primecalculator_0 =>
                        \number_0\ := \PrimeCalcDataIn_0\;
                        \num2\   := 2;  
                        \PrimeCalcDataOut_0\ <= \Primecalc_result_0\;
                        state_SM_Primecalculator_0 <= ST003_Primecalculator_0;
                        
                    when ST003_Primecalculator_0 =>
                        \num_0\    := \number_0\ /2;
                        if (not (\number_0\ mod \num2\ /= 0)) then
                            state_SM_Primecalculator_0 <= ST004_Primecalculator_0;
                        else
                            state_SM_Primecalculator_0 <= ST006_Primecalculator_0;
                        end if;
                        
                    when ST004_Primecalculator_0 =>
                        \Primecalc_result_0\ <= '0';
                        state_SM_Primecalculator_0 <= ST005_Primecalculator_0;
                        
                    when ST005_Primecalculator_0 =>
                        \PrimeCalcDataOut_0\ <= \Primecalc_result_0\;
                        state_SM_Primecalculator_0 <= ST008_Primecalculator_0;
                        
                    when ST006_Primecalculator_0 =>
                        \num2\ := \num2\ + 1;
                        if \num2\ <= \num_0\ then
                            state_SM_Primecalculator_0 <= ST003_Primecalculator_0;
                        else 
                            state_SM_Primecalculator_0 <= ST007_Primecalculator_0;
                        end if;
                        
                    when ST007_Primecalculator_0 =>
                        \Primecalc_result_0\ <= '1';
                        state_SM_Primecalculator_0 <= ST005_Primecalculator_0;
                        
                    when ST008_Primecalculator_0 =>
                        \result_0\ <= \PrimeCalcDataOut_0\;
                        state_SM_Primecalculator_0 <= ST009_Primecalculator_0;  
                        
                    when ST009_Primecalculator_0 =>  
                        Primecalc_Finished_0 <= '1';                              
                        state_SM_Primecalculator_0 <= ST000_Primecalculator_0;  
                             
                    when others => null;  
                                                         
                end case;
            end if;
        end if;                                                      
    end process;  */
        }


        private Value CreateStateValue(int index)
        {
            return CreateStateName(index).ToVhdlIdValue();
        }
    }
}
