using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICSharpCode.NRefactory.CSharp
{
    public static class AstNodeExtensions
    {
        public static AstNode FindFirstNonParenthesizedExpressionParent(this AstNode node)
        {
            var parent = node.Parent;

            while (parent is ParenthesizedExpression && parent != null)
            {
                parent = parent.Parent;
            }

            return parent;
        }
    }
}
