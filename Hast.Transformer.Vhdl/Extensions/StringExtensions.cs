using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static class StringExtensions
    {
        public static bool IsTaskFromResultMethodName(this string name)
        {
            return name.Contains("System.Threading.Tasks.Task::FromResult");
        }

        public static bool IsTaskCompletedTaskPropertyName(this string name)
        {
            return name.Contains("System.Threading.Tasks.Task::CompletedTask");
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
            return name.Contains("/<>") && name.Contains("__DisplayClass");
        }
    }
}
