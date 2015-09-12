using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hast.VhdlBuilder.Representation;

namespace Hast.VhdlBuilder.Extensions
{
    public static class VhdlElementEnumerableExtensions
    {
        public static string ToVhdl(this IEnumerable<IVhdlElement> elements, IVhdlGenerationContext vhdlGenerationContext)
        {
            return elements.ToVhdl(vhdlGenerationContext, string.Empty);
        }

        public static string ToVhdl(
            this IEnumerable<IVhdlElement> elements,
            IVhdlGenerationContext vhdlGenerationContext,
            string elementTerminator)
        {
            if (elements == null || !elements.Any()) return string.Empty;

            var builder = new StringBuilder();

            foreach (var element in elements)
            {
                builder.Append(
                    vhdlGenerationContext.IndentIfShouldFormat() + 
                    element.ToVhdl(vhdlGenerationContext) + 
                    elementTerminator +
                    vhdlGenerationContext.NewLineIfShouldFormat());
            }

            return builder.ToString();
        }
    }
}
