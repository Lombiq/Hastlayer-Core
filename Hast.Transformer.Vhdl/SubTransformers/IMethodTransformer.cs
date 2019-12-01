using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IMethodTransformer : IDependency
    {
        Task<IMemberTransformerResult> Transform(MethodDeclaration method, IVhdlTransformationContext context);
    }
}
