using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.Transformer.Models;
using Hast.Common.Configuration;
using Hast.VhdlBuilder.Representation.Expression;
using Hast.VhdlBuilder.Representation.Declaration;
using ICSharpCode.Decompiler.Ast;
using Hast.Common.Numerics;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class SpecialOperationInvocationTransformer : ISpecialOperationInvocationTransformer
    {
        private readonly IBinaryOperatorExpressionTransformer _binaryOperatorExpressionTransformer;
        private readonly ITypeConverter _typeConverter;


        public SpecialOperationInvocationTransformer(
            IBinaryOperatorExpressionTransformer binaryOperatorExpressionTransformer, 
            ITypeConverter typeConverter)
        {
            _binaryOperatorExpressionTransformer = binaryOperatorExpressionTransformer;
            _typeConverter = typeConverter;
        }


        public bool IsSpecialOperationInvocation(InvocationExpression expression)
        {
            var targetMethodName = expression.GetFullName();

            return TryGetSimdOperation(targetMethodName) != null;
        }

        public IVhdlElement TransformSpecialOperationInvocation(
            InvocationExpression expression,
            IEnumerable<IVhdlElement> transformedParameters,
            ISubTransformerContext context)
        {
            if (!IsSpecialOperationInvocation(expression))
            {
                throw new InvalidOperationException(
                    "The given expression (" + expression.ToString() + ") is not a special operation invocation.");
            }

            var targetMethodName = expression.GetFullName();

            var simdOperation = TryGetSimdOperation(targetMethodName);

            if (string.IsNullOrEmpty(simdOperation))
            {
                throw new NotSupportedException(
                    "No transformer logic exists for the following special operation invocation: " + expression.ToString());
            }

            // Transforming the operation to parallel signal-using operations.

            // The last argument for SIMD operations is always the max degree of parallelism.
            var maxDegreeOfParallelism = int.Parse(((Value)transformedParameters.Last()).Content);

            var vector1 = (DataObjectReference)transformedParameters.First();
            var vector2 = (DataObjectReference)transformedParameters.Skip(1).First();
            var binaryOperations = new List<IPartiallyTransformedBinaryOperatorExpression>();

            BinaryOperatorType simdBinaryOperator;
            switch (simdOperation)
            {
                case nameof(SimdOperations.AddVectors):
                    simdBinaryOperator = BinaryOperatorType.Add;
                    break;
                case nameof(SimdOperations.SubtractVectors):
                    simdBinaryOperator = BinaryOperatorType.Subtract;
                    break;
                case nameof(SimdOperations.MultiplyVectors):
                    simdBinaryOperator = BinaryOperatorType.Multiply;
                    break;
                case nameof(SimdOperations.DivideVectors):
                    simdBinaryOperator = BinaryOperatorType.Divide;
                    break;
                default:
                    throw new NotSupportedException("The SIMD operation " + simdOperation + " is not supported.");
            }

            // The result type of each artificial BinaryOperatorExpression should be the same as the SIMD method call's
            // return array type's element type.
            var resultElemenetType = expression.Annotation<TypeInformation>().ExpectedType.GetElementType();
            var resultElementTypeInformation = new TypeInformation(resultElemenetType, resultElemenetType);

            for (int i = 0; i < maxDegreeOfParallelism; i++)
            {
                var binaryOperatorExpression = new BinaryOperatorExpression(
                        new IndexerExpression(
                            expression.Arguments.First().Clone(), // The first array's reference.
                            new PrimitiveExpression(i)), // The expression object can't be re-used below.
                        simdBinaryOperator,
                        new IndexerExpression(
                            expression.Arguments.Skip(1).First().Clone(), // The second array's reference.
                            new PrimitiveExpression(i)));

                binaryOperatorExpression.AddAnnotation(resultElementTypeInformation);

                var indexValue = new Value { DataType = KnownDataTypes.UnrangedInt, Content = i.ToString() };

                binaryOperations.Add(new PartiallyTransformedBinaryOperatorExpression
                {
                    BinaryOperatorExpression = binaryOperatorExpression,
                    LeftTransformed = new ArrayElementAccess { Array = vector1, IndexExpression = indexValue },
                    RightTransformed = new ArrayElementAccess { Array = vector2, IndexExpression = indexValue }
                });
            }


            var stateMachine = context.Scope.StateMachine;
            var preTransformationStateCount = stateMachine.States.Count;

            var resultReferences = _binaryOperatorExpressionTransformer
                .TransformParallelBinaryOperatorExpressions(binaryOperations, context);

            // If no new states were added, i.e. the operation wasn't a multi-cycle one with wait states, then we add
            // a new state here: this is needed because accessing the results (since they are assigned to signales) 
            // should always happen in a separate state.
            if (stateMachine.States.Count == preTransformationStateCount)
            {
                var currentBlock = context.Scope.CurrentBlock;

                var resultAccessBlock = new InlineBlock();
                var resultAccessStateIndex = stateMachine.AddState(resultAccessBlock);
                currentBlock.Add(stateMachine.CreateStateChange(resultAccessStateIndex));
                currentBlock.ChangeBlockToDifferentState(resultAccessBlock, resultAccessStateIndex);
            }

            // Returning the results as an array initialization value (i.e.: array := (result1, result2);)
            return new Value
            {
                DataType = _typeConverter.ConvertTypeReference(expression.GetActualTypeReference()),
                EvaluatedContent = new InlineBlock(resultReferences)
            };
        }


        private string TryGetSimdOperation(string targetMethodName)
        {
            var simdOperationsClassFullNamePrefix = typeof(SimdOperations).FullName + "::";
            var simdOperations = new[]
            {
                nameof(SimdOperations.AddVectors),
                nameof(SimdOperations.SubtractVectors),
                nameof(SimdOperations.MultiplyVectors),
                nameof(SimdOperations.DivideVectors)
            };

            for (int i = 0; i < simdOperations.Length; i++)
            {
                if (targetMethodName.Contains(simdOperationsClassFullNamePrefix + simdOperations[i]))
                {
                    return simdOperations[i];
                }
            }

            return null;
        }


        private class PartiallyTransformedBinaryOperatorExpression : IPartiallyTransformedBinaryOperatorExpression
        {
            public BinaryOperatorExpression BinaryOperatorExpression { get; set; }
            public IVhdlElement LeftTransformed { get; set; }
            public IVhdlElement RightTransformed { get; set; }
        }
    }
}
