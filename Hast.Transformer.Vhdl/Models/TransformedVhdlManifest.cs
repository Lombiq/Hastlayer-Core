using System.Collections.Generic;
using Hast.Layer;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    internal class TransformedVhdlManifest : ITransformedVhdlManifest
    {
        public VhdlManifest Manifest { get; set; }
        public MemberIdTable MemberIdTable { get; set; }
        public IEnumerable<ITransformationWarning> Warnings { get; set; }
    }
}
