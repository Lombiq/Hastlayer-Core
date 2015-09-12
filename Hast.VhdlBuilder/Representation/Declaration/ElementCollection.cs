using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    /// <summary>
    /// A non-semantic container of VHDL elements.
    /// </summary>
    [DebuggerDisplay("{ToVhdl()}")]
    public class ElementCollection : IVhdlElement
    {
        public List<IVhdlElement> Elements { get; set; }


        public ElementCollection()
        {
            Elements = new List<IVhdlElement>();
        }


        public string ToVhdl(IVhdlGenerationContext vhdlGenerationContext)
        {
            // Just concatenating elements.
            var stringBuilder = new StringBuilder();

            foreach (var element in Elements)
            {
                stringBuilder.Append(element.ToVhdl(vhdlGenerationContext));
            }

            return stringBuilder.ToString();
        }
    }
}
