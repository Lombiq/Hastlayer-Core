using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Transformer for processing fields in compiler-generated DisplayClasses.
    /// </summary>
    public interface IDisplayClassFieldTransformer : IDependency
    {
        /// <summary>
        /// Determines if the <paramref name="field"/> belongs to a DisplayClasses.
        /// </summary>
        bool IsDisplayClassField(FieldDeclaration field);

        /// <summary>
        /// Transforms the field declaration into relevant VHDL code.
        /// </summary>
        Task<IMemberTransformerResult> TransformAsync(FieldDeclaration field, IVhdlTransformationContext context);
    }
}
