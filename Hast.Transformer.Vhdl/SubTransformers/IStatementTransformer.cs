using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IStatementTransformer : IDependency
    {
        void Transform(Statement statement, ISubTransformerContext context);
    }
}
