using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Helpers
{
    public static class SimpleMemoryAssignmentHelper
    {
        public static bool IsRead4BytesAssignment(AssignmentExpression assignmentExpression) =>
            assignmentExpression.Left.GetActualTypeReference()?.IsArray == true &&
            assignmentExpression.Right.Is<InvocationExpression>(invocation =>
                invocation.IsSimpleMemoryInvocation() &&
                invocation.Target.Is<MemberReferenceExpression>(memberReference => memberReference.MemberName == "Read4Bytes") &&
                // Excluding the batched overload.
                invocation.Arguments.Count == 1);

        public static bool IsBatchedReadAssignment(AssignmentExpression assignmentExpression, out int cellCount)
        {
            var cellCountOut = 0;
            InvocationExpression readInvocation = null;

            var result =
                assignmentExpression.Left.GetActualTypeReference()?.IsArray == true &&
                assignmentExpression.Right.Is(invocation =>
                    invocation.IsSimpleMemoryInvocation() &&
                    invocation.Arguments.Count == 2, out readInvocation);

            if (result)
            {
                readInvocation.Arguments.Last()
                    .Is<PrimitiveExpression>(primitive => int.TryParse(primitive.Value.ToString(), out cellCountOut));
            }

            cellCount = cellCountOut;
            return result;
        }
    }
}
