using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface ITypeConverter : IDependency
    {
        DataType ConvertTypeReference(TypeReference typeReference);
        DataType ConvertAstType(AstType type);
        DataType ConvertAndDeclareAstType(AstType type, IDeclarableElement declarable);
    }
}
