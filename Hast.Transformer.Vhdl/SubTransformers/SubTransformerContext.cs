using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface ISubTransformerContext
    {
        IVhdlTransformationContext TransformationContext { get; }
        ISubTransformerScope Scope { get; }
    }


    public interface ISubTransformerScope
    {
        MethodDeclaration Method { get; }
        MethodStateMachine StateMachine { get; }
        ICurrentBlock CurrentBlock { get; }
    }


    public interface ICurrentBlock
    {
        void Add(IVhdlElement element);
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
        public MethodStateMachine StateMachine { get; set; }
        public ICurrentBlock CurrentBlock { get; set; }
    }


    public class CurrentBlock : ICurrentBlock
    {
        private IBlockElement _currentBlock;


        public CurrentBlock(IBlockElement currentBlock)
        {
            _currentBlock = currentBlock;
        }

        public CurrentBlock()
        {
            _currentBlock = new InlineBlock();
        }

        public void Add(IVhdlElement element)
        {
            _currentBlock.Body.Add(element);
        }

        public void ChangeBlock(IBlockElement newBlock)
        {
            _currentBlock = newBlock;
        }
    }
}
