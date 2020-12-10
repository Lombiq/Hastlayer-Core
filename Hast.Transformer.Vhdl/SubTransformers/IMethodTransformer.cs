using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IMethodTransformer : IDependency
    {
        Task<IMemberTransformerResult> TransformAsync(MethodDeclaration method, IVhdlTransformationContext context);
    }
}
