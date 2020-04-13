using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IMethodTransformer : IDependency
    {
        Task<IMemberTransformerResult> Transform(MethodDeclaration method, IVhdlTransformationContext context);
    }
}
