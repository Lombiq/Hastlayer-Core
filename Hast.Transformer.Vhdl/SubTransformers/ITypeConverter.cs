using Hast.Transformer.Models;
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
        DataType ConvertTypeReference(
            TypeReference typeReference,
            ITypeDeclarationLookupTable typeDeclarationLookupTable);

        DataType ConvertAstType(AstType type, ITypeDeclarationLookupTable typeDeclarationLookupTable);
        DataType ConvertAndDeclareAstType(
            AstType type, 
            IDeclarableElement declarable,
            ITypeDeclarationLookupTable typeDeclarationLookupTable);
    }


    public static class TypeConvertedExtensions
    {
        public static DataType ConvertParameterType(
            this ITypeConverter typeConverter, 
            ParameterDeclaration parameter,
            ITypeDeclarationLookupTable typeDeclarationLookupTable)
        {
            var parameterType = parameter.Annotation<ParameterDefinition>().ParameterType;

            // This is an out or ref parameter.
            if (parameterType.IsByReference)
            {
                parameterType = ((ByReferenceType)parameterType).ElementType;
            }

            if (!parameterType.IsArray)
            {
                return typeConverter.ConvertTypeReference(parameterType, typeDeclarationLookupTable);
            }

            return ArrayHelper.CreateArrayInstantiation(
                typeConverter.ConvertTypeReference(parameterType.GetElementType(), typeDeclarationLookupTable),
                parameter.Annotation<ArrayLength>().Length);
        }
    }
}
