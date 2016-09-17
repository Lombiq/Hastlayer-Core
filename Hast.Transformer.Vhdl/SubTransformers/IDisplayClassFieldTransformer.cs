using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    /// <summary>
    /// Transformer for processing fields of compiler-generated DisplayClasses.
    /// </summary>
    public interface IDisplayClassFieldTransformer : IDependency
    {
        bool CanTransform(FieldDeclaration field);
        Task<IMemberTransformerResult> Transform(FieldDeclaration field, IVhdlTransformationContext context);
    }
}
