using System.Linq;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class InvocationExpressionExtensions
    {
        public static string GetTargetMemberFullName(this InvocationExpression expression) =>
            expression.GetReferencedMemberFullName();


        /// <summary>
        /// Checks whether the invocation is a Task.Factory.StartNew call like:
        /// array[i] = Task.Factory.StartNew<bool>(new Func<object, bool>(this.<ParallelizedArePrimeNumbers2>b__9_0), num3);
        /// </summary>
        public static bool IsShorthandTaskStart(this InvocationExpression expression) =>
            expression.Target.Is<MemberReferenceExpression>(memberReference => memberReference.IsTaskStartNew()) &&
            expression.Arguments.First().Is<ObjectCreateExpression>(objectCreate =>
                objectCreate.Type.GetFullName().Contains("Func"));
    }
}
