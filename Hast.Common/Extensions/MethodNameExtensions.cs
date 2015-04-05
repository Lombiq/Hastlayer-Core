﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hast.Common.Extensions
{
    // Not in the same namespace as string so it only appears when you need it.
    public static class MethodNameExtensions
    {
        /// <summary>
        /// Creates an alternate versions of a method name if the full method name contains both a class and an interface reference (as it is with
        /// explicitly implemented methods).
        /// </summary>
        /// <remarks>
        /// E.g. a method name as stored in the hardware description can be:
        /// "System.Int32 Hast.Tests.TestAssembly1.ComplexTypes.ComplexTypeHierarchy::Hast.Tests.TestAssembly1.ComplexTypes.IInterface1.Interface1Method1(System.Int32)"
        /// We create two alternates from this:
        /// 1) "System.Int32 Hast.Tests.TestAssembly1.ComplexTypes.ComplexTypeHierarchy::Interface1Method1(System.Int32)"
        /// 2) "System.Int32 Hast.Tests.TestAssembly1.ComplexTypes.IInterface1::Interface1Method1(System.Int32)"
        /// </remarks>
        /// <returns>Alternate method names, if any.</returns>
        public static IEnumerable<string> GetMethodNameAlternates(this string methodFullName)
        {
            var sides = methodFullName.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

            // If there are no dots before the method name that means this full name doesn't contain an interface reference.
            if (sides.Length != 2 || sides[1].IndexOf('.') == -1 || sides[1].IndexOf('.') > sides[1].IndexOf('(')) return Enumerable.Empty<string>();

            var methodName = Regex.Match(methodFullName, @"\.([a-z0-9]*)\(", RegexOptions.Compiled | RegexOptions.IgnoreCase).Groups[1];
            var returnType = sides[0].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            return new[]
            {
                // 1. alternate:
                sides[0] + "::" + sides[1].Substring(sides[1].IndexOf(methodName + "(")),
                // 2. alternate:
                returnType + " " + sides[1].Replace("." + methodName + "(", "::" + methodName + "(")
            };
        }
    }
}
