using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface ITypeConversionTransformer : IDependency
    {
        /// <summary>
        /// In VHDL the operands of binary operations should have the same type, so we need to do a type conversion if 
        /// necessary.
        /// </summary>
        IVhdlElement ImplementTypeConversionForBinaryExpression(
            BinaryOperatorExpression binaryOperatorExpression,
            IVhdlElement variableReference,
            ISubTransformerContext context);

        IVhdlElement ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement variableReference);
    }
}
