using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hast.VhdlBuilder.Representation;

namespace Hast.VhdlBuilder.Extensions
{
    public static class VhdlElementEnumerableExtensions
    {
        public static string ToVhdl(this IEnumerable<IVhdlElement> elements, IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return elements.ToVhdl(vhdlGenerationOptions, string.Empty);
        }

        public static string ToVhdl(
            this IEnumerable<IVhdlElement> elements,
            IVhdlGenerationOptions vhdlGenerationOptions,
            string elementTerminator,
            string lastElementTerminator = null)
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
