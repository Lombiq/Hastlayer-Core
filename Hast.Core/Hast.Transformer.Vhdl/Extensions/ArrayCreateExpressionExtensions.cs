using System;
using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class ArrayCreateExpressionExtensions
    {
        public static int GetStaticLength(this ArrayCreateExpression expression)
        {
            // The array has its length explicitly specified, i.e. new int[5]-stlye.
            if (expression.Arguments.Any())
            {
                var lengthArgument = expression.Arguments.Single();

                if (!(lengthArgument is PrimitiveExpression))
                {
                    throw new NotSupportedException(
                        "Only arrays with statically defined dimension length are supported. Consider adding the dimension sizes directly into the array initialization or use a const field. The dynamically sized array creation was: " +
                        expression.ToString() + " (in the following member: " + 
                        expression.FindFirstParentEntityDeclaration().GetFullName() + ").");
                }

                return int.Parse(((PrimitiveExpression)lengthArgument).Value.ToString()); 
            }
            // The array is initialized in-place, i.e. new[] { 1, 2, 3 }-style.
            else
            {
                return expression.Initializer.Elements.Count;
            }
        }

        public static bool HasInitializer(this ArrayCreateExpression expression) => expression.Initializer.Elements.Count != 0;

        public static AstType GetElementType(this ArrayCreateExpression expression)
        {
            // Sometimes instead of 
            // new Task<uint>[2]
            // an array instantiation will be in form of 
            // new Task<bool>[2][] {}
            // even if the task is still one-dimensional. In this case its type will be a ComposedType.
            return expression.Type is ComposedType ? ((ComposedType)expression.Type).BaseType : expression.Type;
        }
    }
}
