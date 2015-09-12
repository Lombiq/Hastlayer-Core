using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation
{
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
        /// Gets whether the generated code will use shorter names for better readable code.
        /// </summary>
        bool UseShortNames { get; }
    }


    public class VhdlGenerationOptions : IVhdlGenerationOptions
    {
        public bool FormatCode { get; set; }
        public bool UseShortNames { get; set; }
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
    }
}
