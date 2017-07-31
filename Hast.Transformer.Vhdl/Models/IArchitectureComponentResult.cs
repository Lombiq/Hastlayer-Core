using System.Collections.Generic;
using Hast.Layer;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation;

namespace Hast.Transformer.Vhdl.Models
{
    public interface IArchitectureComponentResult
    {
        IArchitectureComponent ArchitectureComponent { get; }
        IVhdlElement Declarations { get; }
        IVhdlElement Body { get; }
        IEnumerable<ITransformationWarning> Warnings { get; }
    }
}
