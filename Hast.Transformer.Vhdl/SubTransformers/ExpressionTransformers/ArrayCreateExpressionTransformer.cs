using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.Transformer.Vhdl.Helpers;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers
{
    public class ArrayCreateExpressionTransformer : IArrayCreateExpressionTransformer
    {
        private readonly ITypeConverter _typeConverter;


        public ArrayCreateExpressionTransformer(ITypeConverter typeConverter)
        {
            _typeConverter = typeConverter;
        }
        
        
        public Value Transform(ArrayCreateExpression expression, IArchitectureComponent component)
        {
            if (expression.Arguments.Count != 1)
            {
                // For the sake of maximal compatibility with synthesis tools we don't allow multi-dimensional
                // arrays, see: http://vhdl.renerta.com/mobile/source/vhd00006.htm "Synthesis tools do generally not 
                // support multidimensional arrays. The only exceptions to this are two-dimensional "vectors of 
                // vectors". Some synthesis tools allow two-dimensional arrays."
                throw new NotSupportedException("Only single-dimensional arrays are supported.");
            }

            var sizeArgument = expression.Arguments.Single();

            if (!(sizeArgument is PrimitiveExpression))
            {
                throw new NotSupportedException("Only arrays with statically defined dimension sizes are supported. Consider adding the dimension sizes directly into the array initialization or use a const field.");
            }

            var size = int.Parse(sizeArgument.ToString());

            if (size < 1)
            {
                throw new InvalidOperationException("An array should have a size greater than 1.");
            }


            // Arrays are tricky: the variable declaration can happen earlier but without an array creation (i.e.
            // at that point the size of the array won't be known) so we have to go back to the variable declaration
            // and set its data type to the unconstrained array instantiation.

            var parentAssignmentExpression = expression.Parent as AssignmentExpression;
            if (parentAssignmentExpression == null || 
                !(parentAssignmentExpression.Left is IdentifierExpression || parentAssignmentExpression.Left is MemberReferenceExpression))
            {
                throw new NotSupportedException("Only array-using constructs where the newly created array is assigned to a variable or member is supported.");
            }

            var elementType = _typeConverter.ConvertAstType(expression.Type);
            var arrayInstantiationType = new UnconstrainedArrayInstantiation
            {
                Name = ArrayTypeNameHelper.CreateArrayTypeName(elementType.Name),
                RangeFrom = 0,
                RangeTo = size - 1
            };

            // Finding the variable or member where the array is used and changing its type to the array instantiation.
            var parentIdentifier = parentAssignmentExpression.Left is IdentifierExpression ?
                ((IdentifierExpression)parentAssignmentExpression.Left).Identifier :
                ((MemberReferenceExpression)parentAssignmentExpression.Left).MemberName;
            var parentDataObjectName = component.CreatePrefixedObjectName(parentIdentifier);
            var parentDataObject = component
                .GetAllDataObjects()
                .Where(dataObject => dataObject.Name == parentDataObjectName)
                .SingleOrDefault();
            // We'll only find the data object if it's in the same architecture component. So e.g. separately transformed
            // fields (possibly in compiler-generated and inner DisplayClasses) won't be handled: those should be dealt
            // with separately.
            if (parentDataObject != null)
            {
                parentDataObject.DataType = arrayInstantiationType; 
            }

            // Initializing the array with the .NET default values so there are no surprises when reading values
            // without setting them previously.
            var arrayInitializationValue = new Value
            {
                DataType = arrayInstantiationType,
                Content = "others => " + elementType.DefaultValue.ToVhdl()
            };

            return arrayInitializationValue;
        }
    }
}
