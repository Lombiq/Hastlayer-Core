using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class ArrayCreateExpressionTransformer : IArrayCreateExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;


        public ArrayCreateExpressionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }


        public UnconstrainedArrayInstantiation CreateArrayInstantiation(
            ArrayCreateExpression expression, 
            IVhdlTransformationContext context)
        {
            return ArrayHelper.CreateArrayInstantiation(
                _typeConverter.ConvertAstType(expression.Type, context), 
                expression.GetStaticLength());
        }

        public IVhdlElement Transform(ArrayCreateExpression expression, ISubTransformerContext context)
        {
            if (expression.Arguments.Any() && expression.Arguments.Count != 1)
            {
                // For the sake of maximal compatibility with synthesis tools we don't allow multi-dimensional
                // arrays, see: http://vhdl.renerta.com/mobile/source/vhd00006.htm "Synthesis tools do generally not 
                // support multidimensional arrays. The only exceptions to this are two-dimensional "vectors of 
                // vectors". Some synthesis tools allow two-dimensional arrays."
                throw new NotSupportedException("Only single-dimensional arrays are supported.");
            }

            var length = expression.GetStaticLength();

            if (length < 1)
            {
                throw new InvalidOperationException("An array should have a length greater than 1.");
            }

            var elementType = _typeConverter.ConvertAstType(
                expression.GetElementType(),
                context.TransformationContext);

            if (elementType.DefaultValue != null)
            {
                // Initializing the array with the .NET default values (so there are no surprises when reading values
                // without setting them previously).
                return ArrayType.CreateDefaultInitialization(
                    ArrayHelper.CreateArrayInstantiation(elementType, length), 
                    elementType);
            }
            else
            {
                // If there's no default value then we can't initialize the array. This is the case when objects
                // are stored in the array and that's no problem, since objects are initialized during instantiation
                // any way.
                return Empty.Instance;
            }
        }
    }
}
