using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer.Vhdl.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Creates a simple dot-delimited name for a full member name, whcih will include the parent types' and the
        /// wrapping namespace's name.
        /// </summary>
        public static string ToSimpleName(this string fullName)
        {
            var simpleName = fullName;

            // Cutting off return type name.
            var firstSpaceIndex = simpleName.IndexOf(' ');
            if (firstSpaceIndex != -1)
            {
                simpleName = simpleName.Substring(firstSpaceIndex + 1);
            }

            // Cutting off everything after an opening bracket (of a method call).
            simpleName = simpleName.Substring(0, simpleName.IndexOf('('));

            // Changing the double colons that delimit a member access to a single dot.
            return simpleName.Replace("::", ".");
        }
    }
}
