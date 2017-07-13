using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Testing
{
    public static class VhdlElementEnumerableExtensions
    {
        public static bool ShouldContain<T>(this IEnumerable<IVhdlElement> elements)
            where T : class, IVhdlElement
        {
            return elements.ShouldContain<T>(null);
        }

        public static bool ShouldContain<T>(
            this IEnumerable<IVhdlElement> elements,
            Expression<Func<T, bool>> predicate)
            where T : class, IVhdlElement
        {
            if (elements.Contains(predicate)) return true;

            throw new VhdlStructureAssertionFailedException(
                "The block of elements didn't contain any " + typeof(T).Name + " elements" +
                (predicate == null ? "." : " matching " + predicate + "."),
                elements.ToVhdl(VhdlGenerationOptions.Debug));
        }

        public static bool ShouldRecursivelyContain(
            this IEnumerable<IVhdlElement> elements,
            Expression<Func<IVhdlElement, bool>> predicate)
        {
            return elements.ShouldRecursivelyContain<IVhdlElement>(predicate);
        }

        public static bool ShouldRecursivelyContain<T>(this IEnumerable<IVhdlElement> elements)
            where T : class, IVhdlElement
        {
            return elements.ShouldRecursivelyContain<T>(null);
        }

        public static bool ShouldRecursivelyContain<T>(
            this IEnumerable<IVhdlElement> elements,
            Expression<Func<T, bool>> predicate)
            where T : class, IVhdlElement
        {
            var queue = new Queue<IVhdlElement>(elements);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();

                if (element.Is(predicate)) return true;

                if (element is IBlockElement)
                {
                    foreach (var subElement in ((IBlockElement)element).Body)
                    {
                        queue.Enqueue(subElement);
                    }
                }
            }

            throw new VhdlStructureAssertionFailedException(
                "The block of elements nor their children didn't contain any " + typeof(T).Name + " elements" +
                (predicate == null ? "." : " matching " + predicate + "."),
                elements.ToVhdl(VhdlGenerationOptions.Debug));
        }

        public static bool Contains<T>(this IEnumerable<IVhdlElement> elements)
            where T : class, IVhdlElement
        {
            return elements.Contains<T>(null);
        }

        public static bool Contains<T>(this IEnumerable<IVhdlElement> elements, Expression<Func<T, bool>> predicate)
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
