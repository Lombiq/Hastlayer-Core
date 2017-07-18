using System.Collections.Generic;
using Hast.Layer;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public interface ITransformedVhdlManifest
    {
        VhdlManifest Manifest { get; }
        MemberIdTable MemberIdTable { get; }
        IEnumerable<ITransformationWarning> Warnings { get; }
    }
}
