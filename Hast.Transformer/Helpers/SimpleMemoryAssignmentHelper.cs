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
                invocation.Target.Is<MemberReferenceExpression>(memberReference => memberReference.MemberName == "Read4Bytes"));
    }
}
