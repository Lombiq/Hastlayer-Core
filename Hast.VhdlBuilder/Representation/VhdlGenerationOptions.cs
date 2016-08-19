using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hast.VhdlBuilder.Representation
{
    /// <summary>
    /// Defines a method or function that will be used to shorten names of VHDL entities when generating VHDL code.
    /// </summary>
    /// <param name="originalName">The original name of the VHDL entity that should be shortened.</param>
    /// <returns>The shortened name.</returns>
    public delegate string NameShortener(string originalName);


    /// <summary>
    /// Provides some configuration options for generating VHDL code. Note that readable code should be only produced
    /// if the result should be handled manually; otherwise for machine processing code shouldn't be formatted.
    /// </summary>
    public interface IVhdlGenerationOptions
    {
        /// <summary>
        /// Gets whether the resulting source code will be formatted in a readable way.
        /// </summary>
        bool FormatCode { get; }

        /// <summary>
        /// Gets whether to omit added comments from the output or include them.
        /// </summary>
        bool OmitComments { get; }

        /// <summary>
        /// Gets the delegate that will be used to shorten the name of VHDL entities when generating VHDL code.
        /// </summary>
        NameShortener NameShortener { get; }
    }


    public class VhdlGenerationOptions : IVhdlGenerationOptions
    {
        /// <summary>
        /// A simple name shortener function. Keep in mind that shortening names with this, while produces more readable
        /// code for debugging, does not guarantee unique names. Be aware that using this can add significant overhead
        /// to VHDL generation (making it take 10 or even more times longer!).
        /// </summary>
        public static NameShortener SimpleNameShortener = originalName =>
            {
                if (string.IsNullOrEmpty(originalName))
                {
                    return string.Empty;
                }

                var newName = originalName;
                var previousNewName = string.Empty;
                // As long as we can find names inside names we'll replace the inner names first.

                while (newName != previousNewName)
                {
                    previousNewName = newName;

                    newName = Regex.Replace(
                        newName,
                        // Detects names in the following patterns:
                        // System.Void Hast.Samples.SampleAssembly.PrimeCalculator::ArePrimeNumbers(Hast.Transformer.SimpleMemory.SimpleMemory)
                        // \System.Void Hast::ExternalInvocationProxy().System.Void Hast.Samples.SampleAssembly.PrimeCalculator::IsPrimeNumber(Hast.Transformer.SimpleMemory.SimpleMemory)._Finished.0\
                        // Will also replace names in names.
                        @"\\?\S+\.\S+ [^\s:]+::[^\s(]+\(\S*?\)(\.\d+)?\\?",
                        match =>
                        {
                            var originalMatch = match.Groups[0].Value;
                            var shortName = originalMatch;

                            // Cutting off return type name.
                            var firstSpaceIndex = shortName.IndexOf(' ');
                            if (firstSpaceIndex != -1)
                            {
                                shortName = shortName.Substring(firstSpaceIndex + 1);
                            }

                            // Cutting off namespace name, type name can be enough.
                            var doubleColonIndex = shortName.IndexOf("::");
                            if (doubleColonIndex != -1)
                            {
                                var namespaceAndClassName = shortName.Substring(0, doubleColonIndex);
                                shortName =
                                    namespaceAndClassName.Substring(namespaceAndClassName.LastIndexOf('.') + 1) +
                                    shortName.Substring(doubleColonIndex);
                            }

                            // Shortening parameter type names to just their type name.
                            if (shortName.Contains('(') && shortName.Contains(')'))
                            {
                                var openingParenthesisIndex = shortName.IndexOf('(');
                                var closingParenthesisIndex = shortName.IndexOf(')');

                                var shortenedParameters = string.Join(",", shortName
                                    .Substring(openingParenthesisIndex + 1, closingParenthesisIndex - openingParenthesisIndex)
                                    .Split(',')
                                    .Select(parameter => parameter.Split('.').Last()));

                                shortName =
                                    shortName.Substring(0, openingParenthesisIndex + 1) +
                                    shortenedParameters +
                                    shortName.Substring(closingParenthesisIndex + 1);
                            }

                            // Keep leading backslash for extended VHDL identifiers.
                            if (originalMatch.StartsWith(@"\") && !shortName.StartsWith(@"\"))
                            {
                                shortName = @"\" + shortName;
                            }

                            // Keep leading dot for concatenated names.
                            if (originalMatch.StartsWith(".") && !shortName.StartsWith("."))
                            {
                                shortName = "." + shortName;
                            }

                            return shortName;
                    },
                    RegexOptions.Compiled);
                }

                return newName;
            };

        private static VhdlGenerationOptions _debugOptions = new VhdlGenerationOptions
        {
            FormatCode = true,
            OmitComments = false,
            NameShortener = SimpleNameShortener
        };
        public static VhdlGenerationOptions Debug { get { return _debugOptions; } }

        public bool FormatCode { get; set; } = true;
        public bool OmitComments { get; set; } = true;
        public NameShortener NameShortener { get; set; } = name => name; // No name shortening by default.
    }


    public static class VhdlGenerationOptionsExtensions
    {
        public static string NewLineIfShouldFormat(this IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return vhdlGenerationOptions.FormatCode ? Environment.NewLine : string.Empty;
        }

        public static string IndentIfShouldFormat(this IVhdlGenerationOptions vhdlGenerationOptions)
        {
            // Using spaces instead of tabs.
            return vhdlGenerationOptions.FormatCode ?
                "    " :
                string.Empty;
        }

        public static string ShortenName(this IVhdlGenerationOptions vhdlGenerationOptions, string originalName)
        {
            if (vhdlGenerationOptions.NameShortener == null) return originalName;
            return vhdlGenerationOptions.NameShortener(originalName);
        }
    }
}
