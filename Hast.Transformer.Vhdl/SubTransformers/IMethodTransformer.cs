using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// A service for transforming method declaration nodes.
    /// </summary>
    public interface IMethodTransformer : IDependency
    {
        /// <summary>
        /// Transforms <paramref name="method"/> into VHDL components.
        /// </summary>
        Task<IMemberTransformerResult> TransformAsync(MethodDeclaration method, IVhdlTransformationContext context);
    }
}
