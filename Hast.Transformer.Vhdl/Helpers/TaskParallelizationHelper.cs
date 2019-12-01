using System.Linq;
using ICSharpCode.Decompiler.CSharp;

namespace Hast.Transformer.Vhdl.Helpers
{
    internal static class TaskParallelizationHelper
    {
        /// <summary>
        /// Drill into the expression to find out which DisplayClass method the Func refers to in a compiler-generated
        /// call like: new Func<object, bool>(this.<ParallelizedArePrimeNumbers2>b__9_0)
        /// </summary>
        public static MemberReferenceExpression GetTargetDisplayClassMemberFromFuncCreation(ObjectCreateExpression funcCreateExpression)
        {
            return ((MemberReferenceExpression)funcCreateExpression.Arguments.Single());
        }
    }
}
