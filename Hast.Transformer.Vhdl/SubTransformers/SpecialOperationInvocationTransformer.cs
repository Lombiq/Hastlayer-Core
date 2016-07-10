using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.SpecialOperations;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.SubTransformers.ExpressionTransformers;
using Hast.Transformer.Models;
using Hast.Common.Configuration;
using Hast.VhdlBuilder.Representation.Expression;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public class SpecialOperationInvocationTransformer : ISpecialOperationInvocationTransformer
    {
        private readonly IBinaryOperatorExpressionTransformer _binaryOperatorExpressionTransformer;


        public SpecialOperationInvocationTransformer(IBinaryOperatorExpressionTransformer binaryOperatorExpressionTransformer)
        {
            _binaryOperatorExpressionTransformer = binaryOperatorExpressionTransformer;
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

            if (!string.IsNullOrEmpty(simdOperation))
            {
                // Transforming the operation to parallel signal-using operations.

                context.TransformationContext
                    .GetTransformerConfiguration()
                    .GetMaxInvocationInstanceCountConfigurationForMember(simdOperation);

                // The last argument for SIMD operations is always the max degree of parallelism.
                var maxDegreeOfParallelism = int.Parse(((Value)transformedParameters.Last()).Content);



                return null;
            }

            throw new NotSupportedException(
                "No transformer logic exists for the following special operation invocation: " + expression.ToString());
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
    }
}
