using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hast.VhdlBuilder.Representation;

namespace System.Collections.Generic
{
    public static class VhdlElementEnumerableExtensions
    {
        public static string ToVhdl<T>(this IEnumerable<T> elements, IVhdlGenerationOptions vhdlGenerationOptions)
            where T : IVhdlElement
        {
            return elements.ToVhdl(vhdlGenerationOptions, string.Empty);
        }

        public static string ToVhdl<T>(
            this IEnumerable<T> elements,
            IVhdlGenerationOptions vhdlGenerationOptions,
            string elementTerminator,
            string lastElementTerminator = null)
            where T : IVhdlElement
        {
            if (elements == null || !elements.Any()) return string.Empty;

            var builder = new StringBuilder();

            foreach (var element in elements.Take(elements.Count() - 1))
            {
                builder.Append(element.ToVhdl(vhdlGenerationOptions) + elementTerminator);
            }

            if (lastElementTerminator == null) lastElementTerminator = elementTerminator;
            builder.Append(elements.Last().ToVhdl(vhdlGenerationOptions) + lastElementTerminator);

            return builder.ToString();
        }
    }
}
