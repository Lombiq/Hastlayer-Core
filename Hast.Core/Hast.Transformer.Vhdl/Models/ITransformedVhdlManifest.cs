using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public interface ITransformedVhdlManifest
    {
        VhdlManifest Manifest { get; }
        MemberIdTable MemberIdTable { get; }
    }
}
