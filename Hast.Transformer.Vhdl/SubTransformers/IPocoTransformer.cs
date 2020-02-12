using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Transformer for processing POCOs (Plain Old C# Object) to handle e.g. properties.
    /// </summary>
    public interface IPocoTransformer : IDependency
    {
        bool IsSupportedMember(AstNode node);
        Task<IMemberTransformerResult> Transform(TypeDeclaration typeDeclaration, IVhdlTransformationContext context);
    }
}
