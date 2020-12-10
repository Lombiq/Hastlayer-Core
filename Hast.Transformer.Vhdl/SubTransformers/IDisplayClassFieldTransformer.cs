using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Transformer for processing fields of compiler-generated DisplayClasses.
    /// </summary>
    public interface IDisplayClassFieldTransformer : IDependency
    {
        bool IsDisplayClassField(FieldDeclaration field);
        Task<IMemberTransformerResult> TransformAsync(FieldDeclaration field, IVhdlTransformationContext context);
    }
}
