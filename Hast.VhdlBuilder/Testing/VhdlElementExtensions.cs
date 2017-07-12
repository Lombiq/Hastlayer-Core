using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;

namespace Hast.VhdlBuilder.Testing
{
    public static class VhdlElementExtensions
    {
        public static bool ShouldBe<T>(this IVhdlElement element)
            where T : class, IVhdlElement
        {
            return element.ShouldBe<T>(null);
        }

        public static bool ShouldBe<T>(this IVhdlElement element, Expression<Func<T, bool>> predicate)
            where T : class, IVhdlElement
        {
            if (element.Is(predicate)) return true;

            throw new VhdlStructureAssertionFailedException(
                "The element is not a(n) " + typeof(T).Name + " element" +
                (predicate == null ? "." : " matching " + predicate + "."),
                element.ToVhdl(VhdlGenerationOptions.Debug));
        }

        public static bool Is<T>(this IVhdlElement element)
            where T : class, IVhdlElement
        {
            return element.Is<T>(null);
        }

        public static bool Is<T>(this IVhdlElement element, Expression<Func<T, bool>> predicate)
            where T: class, IVhdlElement
        {
            var target = element as T;
            return target != null && (predicate == null || predicate.Compile()(target));
        }
    }
}
