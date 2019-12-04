using ICSharpCode.Decompiler.CSharp.Syntax;
using System.Text.RegularExpressions;

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
        /// Checks whether the string looks like the name of a compiler-generated class generated from an F# closure.
        /// </summary>
        /// <example>
        /// Such a name is like following: Run@28
        /// </example>
        // // A class name containing "@" would be invalid in standard C#, so this is a fairly safe bet.
        public static bool IsClosureClassName(this string name) => Regex.IsMatch(name, @".+\@\d+", RegexOptions.Compiled);

        /// <summary>
        /// Checks whether the string looks like the name of a compiler-generated DisplayClass from C# or one
        /// generated from an F# closure.
        /// </summary>
        /// <example>
        /// Such a name is like following: 
        /// "Hast.Samples.SampleAssembly.PrimeCalculator/<>c__DisplayClass9_0"
        /// "Hast.Samples.SampleAssembly.HastlayerOptimizedAlgorithm/<>c"
        /// Run@28
        /// </example>
        public static bool IsDisplayOrClosureClassName(this string name) =>
            // A class name containing "<>" would be invalid in standard C#, so this is a fairly safe bet.
            name.Contains("/<>c") ||
            name.IsClosureClassName();

        /// <summary>
        /// Checks whether the string looks like the name of a compiler-generated DisplayClass member.
        /// </summary>
        /// <example>
        /// Such a name is like following: 
        /// "System.UInt32[] Hast.Samples.SampleAssembly.PrimeCalculator/<>c__DisplayClass2::numbers"
        /// "System.UInt32 Hast.Samples.FSharpSampleAssembly.FSharpParallelAlgorithmContainer/Run@28::Invoke(System.UInt32)"
        /// </example>
        public static bool IsDisplayOrClosureClassMemberName(this string name) => 
            name.IsDisplayOrClosureClassName() && name.Contains("::");

        /// <summary>
        /// Checks whether the string looks like the name of a compiler-generated method that was created in place of a
        /// lambda expression in the original class (not in a DisplayClass).
        /// </summary>
        /// <example>
        /// Such a name is like:
        /// "System.Boolean Hast.Samples.SampleAssembly.PrimeCalculator::<ParallelizedArePrimeNumbers2>b__9_0(System.Object)"
        /// or: 
        /// "Hast.Samples.SampleAssembly.ImageContrastModifier/PixelProcessingTaskOutput Hast.Samples.SampleAssembly.ImageContrastModifier::<ChangeContrast>b__6_0(Hast.Samples.SampleAssembly.ImageContrastModifier/PixelProcessingTaskInput)"
        /// </example>
        public static bool IsInlineCompilerGeneratedMethodName(this string name) =>
            // A name where before the "<" there is nothing is invalid in standard C#, so this is a fairly safe bet.
            Regex.IsMatch(name, "^.+?::<.+>.+__\\d_\\d\\(", RegexOptions.Compiled);

        /// <summary>
        /// Determines whether the string looks like the name of a compiler-generated field that backs an auto-property.
        /// </summary>
        /// <example>
        /// Such a field's name looks like "<Number>k__BackingField". It will contain the name of the property.
        /// </example>
        public static bool IsBackingFieldName(this string name) => Regex.IsMatch(name, "<(.*)>.*BackingField");

        /// <summary>
        /// Converts the full name of a property-backing auto-generated field's name to the corresponding property's
        /// name.
        /// </summary>
        /// <remarks>
        /// Such a field's name looks like 
        /// "System.UInt32 Hast.TestInputs.Various.ConstantsUsingCases/ArrayHolder1::<ArrayLength>k__BackingField".
        /// It will contain the name of the property. This needs to be converted into the corresponding full property name:
        /// "System.UInt32 Hast.TestInputs.Various.ConstantsUsingCases/ArrayHolder1::ArrayLength()"
        /// </remarks>
        public static string ConvertFullBackingFieldNameToPropertyName(this string name) =>
             name.ConvertSimpleBackingFieldNameToPropertyName() + "()";

        /// <summary>
        /// Converts the simple name of a property-backing auto-generated field's name to the corresponding property's
        /// name.
        /// </summary>
        /// <remarks>
        /// Such a field's name looks like 
        /// "<Number>k__BackingField".
        /// It will contain the name of the property. This needs to be converted into the corresponding simple property
        /// name: "Number".
        /// </remarks>
        public static string ConvertSimpleBackingFieldNameToPropertyName(this string name) =>
             Regex.Replace(name, "<(.*)>.*BackingField", match => match.Groups[1].Value);

        /// <summary>
        /// Determines whether the string looks like the name of a constructor.
        /// </summary>
        public static bool IsConstructorName(this string name) => name.Contains(".ctor");

        /// <summary>
        /// Adds the full name of the given node's parent entity to the message string. Useful in exception message for
        /// example.
        /// </summary>
        public static string AddParentEntityName(this string message, AstNode node)
        {
            var parentEntity = node.FindFirstParentEntityDeclaration();
            if (parentEntity == null) return message;
            return message + " Parent entity where the affected code is: " + parentEntity.GetFullName();
        }
    }
}
