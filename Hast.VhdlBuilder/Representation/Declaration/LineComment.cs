using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// A VHDL comment line.
    /// </summary>
    /// <seealso cref="BlockComment"/>
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class LineComment : IVhdlElement
    {
        public string Text { get; set; }


        public LineComment(string text)
        {
            Text = text;
        }
        
        
        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            // There are no block comments in VHDL so if the code is not formatted there can't be any comments.
            if (!vhdlGenerationOptions.FormatCode) return string.Empty;

            return "-- " + Text + vhdlGenerationOptions.NewLineIfShouldFormat();
        }
    }
}
