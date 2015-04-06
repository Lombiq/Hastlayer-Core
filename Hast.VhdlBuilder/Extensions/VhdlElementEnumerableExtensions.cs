using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.VhdlBuilder.Extensions
{
    public static class VhdlElementEnumerableExtensions
    {
        public static string ToVhdl(this IEnumerable<IVhdlElement> elements)
        {
            if (elements == null || elements.Count() == 0) return string.Empty;

            var builder = new StringBuilder();
            foreach (var element in elements) builder.Append(element.ToVhdl());
            return builder.ToString();
        }
    }
}
