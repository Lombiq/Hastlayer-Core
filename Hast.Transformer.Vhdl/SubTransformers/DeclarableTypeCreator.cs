using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class DeclarableTypeCreator : IDeclarableTypeCreator
    {
        private readonly ITypeConverter _typeConverter;


        public DeclarableTypeCreator(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public DataType CreateDeclarableType(AstNode valueHolder, AstType type, IVhdlTransformationContext context)
        {
            if (type.IsArray())
            {
                return ArrayHelper.CreateArrayInstantiation(
                    _typeConverter.ConvertAstType(((ComposedType)type).BaseType, context),
                    context.ArraySizeHolder.GetSizeOrThrow(valueHolder).Length);
            }
            else
            {
                return _typeConverter.ConvertAstType(type, context);
            }
        }
    }
}
