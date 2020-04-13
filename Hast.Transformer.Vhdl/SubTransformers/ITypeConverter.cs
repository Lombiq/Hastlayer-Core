using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface ITypeConverter : IDependency
    {
        DataType ConvertType(
            IType type,
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
            var parameterType = parameter.GetActualType();

            // This is an out or ref parameter.
            if (parameterType.IsByRefLike)
            {
                parameterType = ((ByReferenceType)parameterType).ElementType;
            }

            if (!parameterType.IsArray())
            {
                return typeConverter.ConvertType(parameterType, context);
            }

            return ArrayHelper.CreateArrayInstantiation(
                typeConverter.ConvertType(parameterType.GetElementType(), context),
                context.ArraySizeHolder.GetSizeOrThrow(parameter).Length);
        }
    }
}
