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
            IVhdlTransformationContext context);

        DataType ConvertAstType(AstType type, IVhdlTransformationContext context);
        DataType ConvertAndDeclareAstType(
            AstType type, 
            IDeclarableElement declarable,
            IVhdlTransformationContext context);
    }


    public static class TypeConvertedExtensions
    {
        public static DataType ConvertParameterType(
            this ITypeConverter typeConverter, 
            ParameterDeclaration parameter,
            IVhdlTransformationContext context)
        {
            var parameterType = parameter.Annotation<ParameterDefinition>().ParameterType;

            // This is an out or ref parameter.
            if (parameterType.IsByReference)
            {
                parameterType = ((ByReferenceType)parameterType).ElementType;
            }

            if (!parameterType.IsArray)
            {
                return typeConverter.ConvertTypeReference(parameterType, context);
            }

            return ArrayHelper.CreateArrayInstantiation(
                typeConverter.ConvertTypeReference(parameterType.GetElementType(), context),
                context.ArraySizeHolder.GetSizeOrThrow(parameter).Length);
        }
    }
}
