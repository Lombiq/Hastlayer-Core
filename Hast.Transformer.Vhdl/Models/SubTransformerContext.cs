using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using System.Collections;
using System.Collections.Generic;

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
        /// Maps the names of variables that store object references to compiler-generated DisplayClasses (created for
        /// lambda expressions) to full DisplayClass names.
        /// </summary>
        IDictionary<string, string> VariableToDisplayClassMappings { get; }

        /// <summary>
        /// Maps the names of variables that store object references to compiler-generated Func<TIn, TOut> objects that
        /// are then populated with references to DisplayClasse methods.
        /// </summary>
        IDictionary<string, string> VariableToDisplayClassMethodMappings { get; }

        /// <summary>
        /// Keeps track of the name of those variables that store references to Tasks and then later the Task results
        /// fetched from them via Task.Result.
        /// </summary>
        ISet<string> TaskReferencingVariableNames { get; }

        /// <summary>
        /// Keeps track of the name of those variables that store references to 
        /// <see cref="System.Threading.Tasks.TaskFactory"/> objects.
        /// </summary>
        ISet<string> TaskFactoryVariableNames { get; }
    }


    public interface ICurrentBlock
    {
        int CurrentStateMachineStateIndex { get; }
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
        public IDictionary<string, string> VariableToDisplayClassMappings { get; } = new Dictionary<string, string>();
        public IDictionary<string, string> VariableToDisplayClassMethodMappings { get; } = new Dictionary<string, string>();
        public ISet<string> TaskReferencingVariableNames { get; } = new HashSet<string>();
        public ISet<string> TaskFactoryVariableNames { get; } = new HashSet<string>();
    }


    public class CurrentBlock : ICurrentBlock
    {
        private readonly IMemberStateMachine _stateMachine;
        private IBlockElement _currentBlock;

        public int CurrentStateMachineStateIndex { get; private set; }

        public decimal RequiredClockCycles
        {
            get
            {
                return _stateMachine.States[CurrentStateMachineStateIndex].RequiredClockCycles;
            }

            set
            {
                _stateMachine.States[CurrentStateMachineStateIndex].RequiredClockCycles = value;
            }
        }


        public CurrentBlock(IMemberStateMachine stateMachine, IBlockElement currentBlock, int stateMachineStateIndex)
            : this(stateMachine)
        {
            _currentBlock = currentBlock;
            CurrentStateMachineStateIndex = stateMachineStateIndex;
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
            CurrentStateMachineStateIndex = stateMachineStateIndex;
            ChangeBlock(newBlock);
        }

        public void ChangeBlock(IBlockElement newBlock)
        {
            _currentBlock = newBlock;
        }
    }
}
