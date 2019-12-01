using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public interface ITypeConversionResult
    {
        IVhdlElement ConvertedFromExpression { get; }
        bool IsLossy { get; }
        bool IsResized { get; }
    }

    public interface IAssignmentTypeConversionResult : ITypeConversionResult
    {
        IDataObject ConvertedToDataObject { get; }
    }


    public interface ITypeConversionTransformer : IDependency
    {
        /// <summary>
        /// In VHDL the operands of binary operations should have the same type, so we need to do a type conversion if 
        /// necessary.
        /// </summary>
        IVhdlElement ImplementTypeConversionForBinaryExpression(
            BinaryOperatorExpression binaryOperatorExpression,
            DataObjectReference variableReference,
            bool isLeft,
            ISubTransformerContext contex);

        IAssignmentTypeConversionResult ImplementTypeConversionForAssignment(
            DataType fromType, 
            DataType toType, 
            IVhdlElement fromExpression, 
            IDataObject toDataObject);

        ITypeConversionResult ImplementTypeConversion(DataType fromType, DataType toType, IVhdlElement fromExpression);
    }
}
