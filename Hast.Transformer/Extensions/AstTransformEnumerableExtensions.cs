using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.Decompiler.Ast.Transforms;

namespace System.Collections.Generic
{
    internal static class AstTransformEnumerableExtensions
    {
        public static IEnumerable<IAstTransform> Without(this IEnumerable<IAstTransform> enumerable, string className)
        {
            return enumerable.Where(item => item.GetType().Name != className);
        }
    }
}
