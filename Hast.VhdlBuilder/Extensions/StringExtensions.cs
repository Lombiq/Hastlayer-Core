using System;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;
using System.Linq;

namespace Hast.VhdlBuilder.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts a string identifier to be used in VHDL as an extended identifier.
        /// </summary>
        public static string ToExtendedVhdlId(this string id)
        {
            if (id.StartsWith(@"\") && id.EndsWith(@"\")) return id;
            return @"\" + id + @"\";
        }

        /// <summary>
        /// Trims the VHDL extended identifier delimiters (backslash characters) from the given string.
        /// </summary>
        public static string TrimExtendedVhdlIdDelimiters(this string id)
        {
            return id.Trim('\\');
        }

        /// <summary>
        /// Converts a string identifier to a VHDL identifier value object as an extended VHDL identifier.
        /// </summary>
        public static Value ToExtendedVhdlIdValue(this string id)
        {
            return id.ToExtendedVhdlId().ToVhdlIdValue();
        }

        /// <summary>
        /// Converts a string identifier to a VHDL identifier value object.
        /// </summary>
        public static Value ToVhdlIdValue(this string id)
        {
            return new IdentifierValue(id);
        }

        /// <summary>
        /// Converts a variable name to a VHDL variable reference.
        /// </summary>
        public static DataObjectReference ToVhdlVariableReference(this string variableName)
        {
            return new DataObjectReference
            {
                DataObjectKind = DataObjectKind.Variable,
                Name = variableName
            };
        }

        public static string IndentLinesIfShouldFormat(this string vhdl, IVhdlGenerationContext vhdlGenerationContext)
        {
            return string.Join(string.Empty, vhdl
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(line => vhdlGenerationContext.IndentIfShouldFormat() + line));
        }
    }
}
