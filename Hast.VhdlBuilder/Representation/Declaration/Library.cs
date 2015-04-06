using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl()}")]
    public class Library : INamedElement
    {
        public string Name { get; set; }
        public List<string> Uses { get; set; }


        public Library()
        {
            Uses = new List<string>();
        }


        public string ToVhdl()
        {
            if (string.IsNullOrEmpty(Name)) return string.Empty;

            var builder = new StringBuilder();

            builder
                .Append("library ")
                .Append(Name)
                .Append(";");

            foreach (var use in Uses)
            {
                builder
                    .Append("use ")
                    .Append(use)
                    .Append(";");
            }

            return builder.ToString();
        }
    }
}
