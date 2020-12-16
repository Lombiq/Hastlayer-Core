using ICSharpCode.Decompiler.TypeSystem;
using System.Linq;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class InvocationExpressionExtensions
    {
        public static string GetTargetMemberFullName(this InvocationExpression expression) =>
            expression.GetReferencedMemberFullName();

        /// <summary>
        /// Checks whether the invocation is a Task.Factory.StartNew call like either of the following:
        /// Task.Factory.StartNew(<>c__DisplayClass4_.<>9__0 ?? (<>c__DisplayClass4_.<>9__0 = <>c__DisplayClass4_.<NameOfTaskStartingMethod>b__0), inputArgument);
        /// Task.Factory.StartNew((Func<object, OutputType>)this.<NameOfTaskStartingMethod>b__6_0, (object)inputArgument);
        /// </summary>
        public static bool IsTaskStart(this InvocationExpression expression) =>
            expression.Target.Is<MemberReferenceExpression>(memberReference => memberReference.IsTaskStartNew()) &&
            (expression.Arguments.First().Is<BinaryOperatorExpression>(binary => binary.GetActualType().IsFunc()) ||
            expression.Arguments.First().Is<CastExpression>(binary => binary.GetActualType().IsFunc()));
    }
}
