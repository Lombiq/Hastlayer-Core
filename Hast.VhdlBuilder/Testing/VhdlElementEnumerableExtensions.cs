using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Testing
{
    public static class VhdlElementEnumerableExtensions
    {
        public static void ShouldContain<T>(this IEnumerable<IVhdlElement> elements, Expression<Func<T, bool>> predicate)
            where T : class, IVhdlElement
        {
            if (Contains(elements, predicate)) return;

            throw new VhdlStructureAssertionFailedException(
                "The block of elements didn't contain any " + typeof(T).Name + " elements matching " + predicate + ".",
                elements.ToVhdl(VhdlGenerationOptions.Debug));
        }

        public static void ShouldRecursivelyContain<T>(this IEnumerable<IVhdlElement> elements, Expression<Func<T, bool>> predicate)
            where T : class, IVhdlElement
        {
            var queue = new Queue<IVhdlElement>(elements);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();

                if (element.Is(predicate)) return;

                if (element is IBlockElement)
                {
                    foreach (var subElement in ((IBlockElement)element).Body)
                    {
                        queue.Enqueue(subElement);
                    }
                }
            }

            throw new VhdlStructureAssertionFailedException(
                "The block of elements nor their children didn't contain any " + typeof(T).Name + " elements matching " + predicate + ".",
                elements.ToVhdl(VhdlGenerationOptions.Debug));
        }

        public static bool Contains<T>(IEnumerable<IVhdlElement> elements, Expression<Func<T, bool>> predicate)
            where T : class, IVhdlElement
        {
            foreach (var element in elements)
            {
                if (element.Is(predicate)) return true;
            }

            return false;
        }
    }
}
