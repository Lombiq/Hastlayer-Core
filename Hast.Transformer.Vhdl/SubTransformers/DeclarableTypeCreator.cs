using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class DeclarableTypeCreator : IDeclarableTypeCreator
    {
        private readonly ITypeConverter _typeConverter;


        public DeclarableTypeCreator(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public DataType CreateDeclarableType(AstNode valueHolder, TypeReference typeReference, IVhdlTransformationContext context)
        {
            if (typeReference.IsArray)
            {
                return ArrayHelper.CreateArrayInstantiation(
                    _typeConverter.ConvertTypeReference(((Mono.Cecil.ArrayType)typeReference).ElementType, context),
                    context.ArraySizeHolder.GetSizeOrThrow(valueHolder).Length);
            }
            else
            {
                return _typeConverter.ConvertTypeReference(typeReference, context);
            }
        }
    }
}
