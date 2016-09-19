using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                        "Only arrays with statically defined dimension length are supported. Consider adding the dimension sizes directly into the array initialization or use a const field.");
                }

                return int.Parse(lengthArgument.ToString()); 
            }
            // The array is initialized in-place, i.e. new[] { 1, 2, 3 }-style.
            else
            {
                return expression.Initializer.Elements.Count;
            }
        }

        public static bool HasInitializer(this ArrayCreateExpression expression)
        {
            return expression.Initializer.Elements.Count != 0;
        }
    }
}
