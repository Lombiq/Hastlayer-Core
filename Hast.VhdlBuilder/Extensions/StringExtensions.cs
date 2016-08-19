using System;
using System.Linq;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.VhdlBuilder.Representation.Expression;

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

        public static Value ToVhdlValue(this string valueString, DataType dataType)
        {
            return new Value { Content = valueString, DataType = dataType };
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

        /// <summary>
        /// Converts a signal name to a VHDL signal reference.
        /// </summary>
        public static DataObjectReference ToVhdlSignalReference(this string signalName)
        {
            return new DataObjectReference
            {
                DataObjectKind = DataObjectKind.Signal,
                Name = signalName
            };
        }

        public static string IndentLinesIfShouldFormat(this string vhdl, IVhdlGenerationOptions vhdlGenerationOptions)
        {
            // Empty new lines won't be indented as they can contain different blocks.
            // A space will be added if no formatting is uses so the code remains syntactically correct even if being
            // just one line.
            return string.Join(vhdlGenerationOptions.NewLineIfShouldFormat(), vhdl
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .Select(line => (!string.IsNullOrEmpty(line) ? vhdlGenerationOptions.IndentIfShouldFormat() : string.Empty) + line)) +
                (vhdlGenerationOptions.FormatCode ? string.Empty : " ");
        }
    }
}
