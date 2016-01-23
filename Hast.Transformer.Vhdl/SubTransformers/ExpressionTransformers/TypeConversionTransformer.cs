using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Logging;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class TypeConversionTransformer : ITypeConversionTransformer
    {
        private readonly ITypeConverter _typeConverter;

        public ILogger Logger { get; set; }


        public TypeConversionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;

            Logger = NullLogger.Instance;
        }


        public IVhdlElement ImplementTypeConversionForBinaryExpression(
            BinaryOperatorExpression binaryOperatorExpression,
            IVhdlElement variableReference,
            ISubTransformerContext context)
        {
            // If the type of an operand can't be determined the best guess is the expression's type.
            var expressionTypeReference = binaryOperatorExpression.GetActualType();
            var expressionType = expressionTypeReference != null ? _typeConverter.ConvertTypeReference(expressionTypeReference) : null;

            var leftTypeReference = binaryOperatorExpression.Left.GetActualType();
            var leftType = leftTypeReference != null ? _typeConverter.ConvertTypeReference(leftTypeReference) : expressionType;

            var rightTypeReference = binaryOperatorExpression.Right.GetActualType();
            var rightType = rightTypeReference != null ? _typeConverter.ConvertTypeReference(rightTypeReference) : expressionType;

            if (leftType == null || rightType == null)
            {
                throw new InvalidOperationException(
                    "The type of the operands of the following expression could't be determined: " +
                    binaryOperatorExpression.ToString());
            }

            if (leftType == rightType) return variableReference;

            // We compare an Expression to an IVhdlElement, doesn't seem right...
            var isLeft = binaryOperatorExpression.Left == variableReference;
            var thisType = isLeft ? leftType : rightType;
            var otherType = isLeft ? rightType : leftType;

            // We need to convert types in a way to keep precision. E.g. converting an int to real is fine, but vica 
            // versa would cause information loss. However excplicit casting in this direction is allowed in CIL so we 
            // need to allow it here as well.
            if (!((thisType == KnownDataTypes.UnrangedInt || thisType == KnownDataTypes.Natural) && otherType == KnownDataTypes.Real))
            {
                Logger.Warning(
                    "Converting from " + thisType.Name +
                    " to " + otherType.Name +
                    " to fix a binary expression. Although valid in .NET this could cause information loss due to rounding. " +
                    "The affected expression: " + binaryOperatorExpression.ToString() +
                    " in method " + context.Scope.Method.GetFullName() + ".");
            }

            return ImplementTypeConversion(thisType, otherType, variableReference);
        }

        public IVhdlElement ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement variableReference)
        {
            if (fromType == toType)
            {
                return variableReference;
            }

            var castInvokation = new Invokation();

            // Trying supported cast scenarios:
            if ((fromType == KnownDataTypes.UnrangedInt || fromType == KnownDataTypes.Natural) && toType == KnownDataTypes.Real)
            {
                castInvokation.Target = new Raw("real");
            }
            else if ((fromType == KnownDataTypes.Real || fromType == KnownDataTypes.Natural) && (toType == KnownDataTypes.UnrangedInt || toType == KnownDataTypes.Natural))
            {
                castInvokation.Target = new Raw("integer");
            }
            else if (fromType == KnownDataTypes.UnrangedInt && toType == KnownDataTypes.Natural)
            {
                castInvokation.Target = new Raw("natural");
            }
            else
            {
                throw new NotSupportedException("Casting from " + fromType.Name + " to " + toType.Name + " is not supported.");
            }

            castInvokation.Parameters.Add(variableReference);

            return castInvokation;
        }
    }
}
