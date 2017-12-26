using System.Linq;
using ICSharpCode.Decompiler.CSharp.Transforms;

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
