using Hast.Transformer.Vhdl.Models;
using Hast.Transformer.Vhdl.StateMachineGeneration;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

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
    }


    public interface ICurrentBlock
    {
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
    }


    public class CurrentBlock : ICurrentBlock
    {
        private readonly IMemberStateMachine _stateMachine;
        private IBlockElement _currentBlock;
        private int _currentStateMachineStateIndex;

        public decimal RequiredClockCycles
        {
            get
            {
                return _stateMachine.States[_currentStateMachineStateIndex].RequiredClockCycles;
            }

            set
            {
                _stateMachine.States[_currentStateMachineStateIndex].RequiredClockCycles = value;
            }
        }


        public CurrentBlock(IMemberStateMachine stateMachine, IBlockElement currentBlock, int stateMachineStateIndex)
            : this(stateMachine)
        {
            _currentBlock = currentBlock;
            _currentStateMachineStateIndex = stateMachineStateIndex;
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
            _currentStateMachineStateIndex = stateMachineStateIndex;
            ChangeBlock(newBlock);
        }

        public void ChangeBlock(IBlockElement newBlock)
        {
            _currentBlock = newBlock;
        }
    }
}
