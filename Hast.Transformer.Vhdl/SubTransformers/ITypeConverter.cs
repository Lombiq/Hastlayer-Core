using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
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


    public static class TypeConvertedExtensions
    {
        public static DataType ConvertParameterType(this ITypeConverter typeConverter, ParameterDeclaration parameter)
        {
            if (!parameter.Type.IsArray())
            {
                return typeConverter.ConvertAstType(parameter.Type);
            }
            else
            {
                return ArrayHelper.CreateArrayInstantiation(
                    typeConverter.ConvertAstType(((ComposedType)parameter.Type).BaseType),
                    parameter.Annotation<ParameterArrayLength>().Length);
            }
        }
    }
}
