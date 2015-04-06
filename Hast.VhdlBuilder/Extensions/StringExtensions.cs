using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return new Value { DataType = KnownDataTypes.Identifier, Content = id };
        }
    }
}
