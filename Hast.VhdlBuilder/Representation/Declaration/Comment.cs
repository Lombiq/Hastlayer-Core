using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// A VHDL comment.
    /// </summary>
    public class Comment : IVhdlElement
    {
        public string Text { get; set; }


        public Comment(string text)
        {
            Text = text;
        }
        
        
        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return "-- " + Text + vhdlGenerationOptions.NewLineIfShouldFormat();
        }
    }
}
