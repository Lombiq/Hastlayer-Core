using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    internal class ArchitectureComponentResult : IArchitectureComponentResult
    {
        public IArchitectureComponent ArchitectureComponent { get; set; }
        public IVhdlElement Declarations { get; set; }
        public IVhdlElement Body { get; set; }
    }
}
