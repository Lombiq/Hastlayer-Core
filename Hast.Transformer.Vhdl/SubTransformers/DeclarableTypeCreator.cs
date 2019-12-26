﻿using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.Semantics;
using ICSharpCode.Decompiler.TypeSystem;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class DeclarableTypeCreator : IDeclarableTypeCreator
    {
        private readonly ITypeConverter _typeConverter;


        public DeclarableTypeCreator(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public DataType CreateDeclarableType(AstNode valueHolder, IType type, IVhdlTransformationContext context)
        {
            if ((valueHolder.GetResolveResult<MemberResolveResult>()?.Member.ReturnType as ITypeDefinition)?
                    .KnownTypeCode == KnownTypeCode.Void)
            {
                return _typeConverter.ConvertAstType(new PrimitiveType("void"), context);
            }
            else if (type.IsArray())
            {
                return ArrayHelper.CreateArrayInstantiation(
                    _typeConverter.ConvertType(type.GetElementType(), context),
                    context.ArraySizeHolder.GetSizeOrThrow(valueHolder).Length);
            }
            else
            {
                return _typeConverter.ConvertType(type, context);
            }
        }
    }
}
