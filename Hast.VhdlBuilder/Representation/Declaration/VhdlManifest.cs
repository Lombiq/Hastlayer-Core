using System;
using System.Collections.Generic;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    public class VhdlManifest : IVhdlElement
    {
        public List<IVhdlElement> Modules { get; } = new List<IVhdlElement>();

        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions) => Modules.ToVhdl(vhdlGenerationOptions);
    }
}
