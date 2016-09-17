using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Helpers;
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
        
        
        public UnconstrainedArrayInstantiation CreateArrayInstantiation(ArrayCreateExpression expression)
        {
            return ArrayHelper.CreateArrayInstantiation(_typeConverter.ConvertAstType(expression.Type), expression.GetStaticLength());
        }

        public Value Transform(ArrayCreateExpression expression, IArchitectureComponent component)
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


            // Arrays are tricky: the variable declaration can happen earlier but without an array creation (i.e.
            // at that point the length of the array won't be known) so we have to go back to the variable declaration
            // and set its data type to the unconstrained array instantiation. This is unless the array is also 
            // immediately initialized (i.e. new[] { 1, 2, 3 }-style).

            var isInitialized = expression.Initializer != null;

            var parentAssignmentExpression = expression.Parent as AssignmentExpression;
            if ((parentAssignmentExpression == null || 
                !(parentAssignmentExpression.Left is IdentifierExpression || parentAssignmentExpression.Left is MemberReferenceExpression)) &&
                !isInitialized)
            {
                throw new NotSupportedException(
                    "Only array-using constructs where the newly created array is assigned to a variable or member is supported.");
            }

            var elementType = _typeConverter.ConvertAstType(expression.Type);
            var arrayInstantiationType = ArrayHelper.CreateArrayInstantiation(elementType, length);

            if (!isInitialized)
            {
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
            }

            // Initializing the array with the .NET default values (so there are no surprises when reading values
            // without setting them previously) or the initialized values.
            var arrayInitializationValue = new Value
            {
                DataType = arrayInstantiationType
            };

            if (isInitialized)
            {
                //arrayInitializationValue.EvaluatedContent = 
            }
            else
            {
                arrayInitializationValue.Content = "others => " + elementType.DefaultValue.ToVhdl();
            }

            return arrayInitializationValue;
        }
    }
}
