using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhdlBuilder
{
    public class Library : IVhdlElement
    {
        public string Name { get; set; }
        public string[] Uses { get; set; }


        public Library()
        {
            Uses = new string[0];
        }


        public string ToVhdl()
        {
            if (String.IsNullOrEmpty(Name)) return String.Empty;

            var builder = new StringBuilder(3 + Uses.Length * 3);

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
