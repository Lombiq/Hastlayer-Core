using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IStatementTransformer : IDependency
    {
        void Transform(Statement statement, ISubTransformerContext context);
    }
}
