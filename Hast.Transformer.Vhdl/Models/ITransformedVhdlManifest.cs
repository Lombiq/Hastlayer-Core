using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.Models
{
    public interface ITransformedVhdlManifest
    {
        VhdlManifest Manifest { get; }
        MemberIdTable MemberIdTable { get; }
    }
}
