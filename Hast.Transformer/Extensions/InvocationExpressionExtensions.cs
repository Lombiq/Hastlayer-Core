using System.Linq;

namespace ICSharpCode.Decompiler.CSharp.Syntax
{
    public static class InvocationExpressionExtensions
    {
        /// <summary>
        /// Retrieves the return type of the method that was invoked in an invocation expression.
        /// </summary>
        public static TypeReference GetReturnTypeReference(this InvocationExpression expression)
        {
            // Looking up the type information that will tell us what the return type of the invocation is. 
            // This might be some nodes up if e.g. there is an immediate cast expression.
            AstNode currentNode = expression;
            while (currentNode.Annotation<TypeInformation>() == null)
            {
                currentNode = currentNode.Parent;
            }

            return currentNode.GetActualType();
        }

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
