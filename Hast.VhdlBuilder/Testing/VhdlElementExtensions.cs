using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;

namespace Hast.VhdlBuilder.Testing
{
    public static class VhdlElementExtensions
    {
        public static void ShouldBe<T>(this IVhdlElement element, Predicate<T> predicate)
            where T : class, IVhdlElement
        {
            if (element.Is(predicate)) return;

            throw new VhdlStructureAssertionFailedException(
                "The element is not a(n) " + typeof(T).Name + ".",
                element.ToVhdl(VhdlGenerationOptions.Debug));
        }

        public static bool Is<T>(this IVhdlElement element, Predicate<T> predicate)
            where T: class, IVhdlElement
        {
            var target = element as T;
            return target != null && predicate(target);
        }
    }
}
