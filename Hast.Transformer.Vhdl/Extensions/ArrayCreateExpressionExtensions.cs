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
            var lengthArgument = expression.Arguments.Single();

            if (!(lengthArgument is PrimitiveExpression))
            {
                throw new NotSupportedException(
                    "Only arrays with statically defined dimension length are supported. Consider adding the dimension sizes directly into the array initialization or use a const field.");
            }

            return int.Parse(lengthArgument.ToString());
        }
    }
}
