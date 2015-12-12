using Hast.Transformer.Vhdl.Models;
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
        ISubProgram SubProgram { get; }
    }


    public class SubTransformerContext : ISubTransformerContext
    {
        public IVhdlTransformationContext TransformationContext { get; set; }
        public ISubTransformerScope Scope { get; set; }
    }


    public class SubTransformerScope : ISubTransformerScope
    {
        public MethodDeclaration Method { get; set; }
        public ISubProgram SubProgram { get; set; }
    }
}
