using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Gets the delegate that will be used to shorten the name of VHDL entities when generating VHDL code.
        /// </summary>
        NameShortener NameShortener { get; }
    }


    public class VhdlGenerationOptions : IVhdlGenerationOptions
    {
        /// <summary>
        /// A simple name shortener function. Keep in mind that shortening names with this, while produces more readable
        /// code for debugging, does not guarantee unique names.
        /// </summary>
        public static NameShortener DefaultNameShortener = originalName =>
            {
                var shortName = originalName;

                // Cutting of return type name.
                var firstSpaceIndex = shortName.IndexOf(' ');
                if (firstSpaceIndex != -1)
                {
                    shortName = shortName.Substring(firstSpaceIndex + 1);
                }

                // Cutting of namespace name, type name can be enough.
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
                if (originalName.StartsWith(@"\") && !shortName.StartsWith(@"\"))
                {
                    shortName = @"\" + shortName;
                }

                return shortName;
            };

        public bool FormatCode { get; set; }
        public NameShortener NameShortener { get; set; }


        public VhdlGenerationOptions()
        {
            // No name shortening by default.
            NameShortener = name => name;
        }
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
