using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class StringExtensions
    {
        /// <summary>
        /// Creates a simple dot-delimited name for a full member name, which will include the parent types' and the
        /// wrapping namespace's name. This can be used where name prefixes are required.
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

        /// <summary>
        /// Checkes whether the string looks like the name of a compiler-generated DisplayClass.
        /// </summary>
        /// <example>
        /// Such a name is like following: 
        /// "Hast.Samples.SampleAssembly.PrimeCalculator/<>c__DisplayClass9_0"
        /// </example>
        public static bool IsDisplayClassName(this string name)
        {
            return name.Contains("/<>") && name.Contains("__DisplayClass");
        }

        /// <summary>
        /// Checkes whether the string looks like the name of a compiler-generated DisplayClass member.
        /// </summary>
        /// <example>
        /// Such a name is like following: 
        /// "System.UInt32[] Hast.Samples.SampleAssembly.PrimeCalculator/<>c__DisplayClass2::numbers"
        /// </example>
        public static bool IsDisplayClassMemberName(this string name)
        {
            return name.IsDisplayClassName() && name.Contains("::");
        }
    }
}
