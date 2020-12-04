using Hast.Layer;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Models
{
    public interface ISubTransformerContext
    {
        IVhdlTransformationContext TransformationContext { get; }
        ISubTransformerScope Scope { get; }
    }

    public interface ISubTransformerScope
    {
        MethodDeclaration Method { get; }
        IMemberStateMachine StateMachine { get; }
        ICurrentBlock CurrentBlock { get; }

        /// <summary>
        /// Gets the names of variables that store object references to compiler-generated DisplayClasses (created for
        /// lambda expressions) to full DisplayClass names.
        /// </summary>
        IDictionary<string, string> VariableNameToDisplayClassNameMappings { get; }

        /// <summary>
        /// Gets the dictionary that keeps track of the name of those variables that store references to Tasks and then later the Task results fetched from them via <see cref="Task{T}.Result"/>.
        /// </summary>
        IDictionary<string, MethodDeclaration> TaskVariableNameToDisplayClassMethodMappings { get; }

        /// <summary>
        /// Gets the dictionary that Keeps track of which invoked state machines were finished in which states. This is needed not to immediately restart a component in the state it was finished.
        /// </summary>
        IDictionary<int, ISet<string>> FinishedInvokedStateMachinesForStates { get; }

        /// <summary>
        /// Gets the label statements to state machine state indices. This is necessary because each label should have its
        /// own state (so it's possible to jump to it).
        /// </summary>
        IDictionary<string, int> LabelsToStateIndicesMappings { get; }

        /// <summary>
        /// Gets any other custom values for the scope.
        /// </summary>
        IDictionary<string, dynamic> CustomProperties { get; }

        /// <summary>
        /// Gets the warnings issued during transformation.
        /// </summary>
        IList<ITransformationWarning> Warnings { get; }
    }

    public interface ICurrentBlock
    {
        int StateMachineStateIndex { get; }
        decimal RequiredClockCycles { get; set; }

        void Add(IVhdlElement element);
        void ChangeBlockToDifferentState(IBlockElement newBlock, int stateMachineStateIndex);
        void ChangeBlock(IBlockElement newBlock);
    }

    public class SubTransformerContext : ISubTransformerContext
    {
        public IVhdlTransformationContext TransformationContext { get; set; }
        public ISubTransformerScope Scope { get; set; }
    }

    public class SubTransformerScope : ISubTransformerScope
    {
        public MethodDeclaration Method { get; set; }
        public IMemberStateMachine StateMachine { get; set; }
        public ICurrentBlock CurrentBlock { get; set; }
        public IDictionary<string, string> VariableNameToDisplayClassNameMappings { get; } = new Dictionary<string, string>();
        public IDictionary<string, MethodDeclaration> TaskVariableNameToDisplayClassMethodMappings { get; } = new Dictionary<string, MethodDeclaration>();
        public IDictionary<int, ISet<string>> FinishedInvokedStateMachinesForStates { get; } = new Dictionary<int, ISet<string>>();
        public IDictionary<string, int> LabelsToStateIndicesMappings { get; set; } = new Dictionary<string, int>();
        public IDictionary<string, dynamic> CustomProperties { get; } = new Dictionary<string, dynamic>();
        public IList<ITransformationWarning> Warnings { get; set; } = new List<ITransformationWarning>();
    }

    public class CurrentBlock : ICurrentBlock
    {
        private readonly IMemberStateMachine _stateMachine;
        private IBlockElement _currentBlock;

        public int StateMachineStateIndex { get; private set; }

        public decimal RequiredClockCycles
        {
            get
            {
                return _stateMachine.States[StateMachineStateIndex].RequiredClockCycles;
            }

            set
            {
                _stateMachine.States[StateMachineStateIndex].RequiredClockCycles = value;
            }
        }

        public CurrentBlock(IMemberStateMachine stateMachine, IBlockElement currentBlock, int stateMachineStateIndex)
            : this(stateMachine)
        {
            _currentBlock = currentBlock;
            StateMachineStateIndex = stateMachineStateIndex;
        }

        public CurrentBlock(IMemberStateMachine stateMachine)
        {
            _currentBlock = new InlineBlock();
            _stateMachine = stateMachine;
        }

        public void Add(IVhdlElement element)
        {
            _currentBlock.Add(element);
        }

        public void ChangeBlockToDifferentState(IBlockElement newBlock, int stateMachineStateIndex)
        {
            StateMachineStateIndex = stateMachineStateIndex;
            ChangeBlock(newBlock);
        }

        public void ChangeBlock(IBlockElement newBlock)
        {
            _currentBlock = newBlock;
        }
    }
}
