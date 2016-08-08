using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    internal class TransformedVhdlManifest : ITransformedVhdlManifest
    {
        public VhdlManifest Manifest { get; set; }
        public MemberIdTable MemberIdTable { get; set; }
    }
}
