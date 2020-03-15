using Hast.Layer;
using Hast.VhdlBuilder.Representation.Declaration;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl.Models
{
    internal class TransformedVhdlManifest : ITransformedVhdlManifest
    {
        public VhdlManifest Manifest { get; set; }
        public MemberIdTable MemberIdTable { get; set; }
        public IEnumerable<ITransformationWarning> Warnings { get; set; }
        public XdcFile XdcFile { get; set; }
    }
}
