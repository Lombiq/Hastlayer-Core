using System.Collections.Generic;
using System.Diagnostics;
using Hast.VhdlBuilder.Extensions;

namespace Hast.VhdlBuilder.Representation.Declaration
{
    [DebuggerDisplay("{ToVhdl(VhdlGenerationOptions.Debug)}")]
    public class Module : IVhdlElement
    {
        public List<Library> Libraries { get; set; }
        public Entity Entity { get; set; }
        public Architecture Architecture { get; set; }


        public Module()
        {
            Libraries = new List<Library>();
        }


        public string ToVhdl(IVhdlGenerationOptions vhdlGenerationOptions)
        {
            return
                Libraries.ToVhdl(vhdlGenerationOptions) + vhdlGenerationOptions.NewLineIfShouldFormat() +
                Entity.ToVhdl(vhdlGenerationOptions) + vhdlGenerationOptions.NewLineIfShouldFormat() +
                Architecture.ToVhdl(vhdlGenerationOptions);
        }
    }
}
