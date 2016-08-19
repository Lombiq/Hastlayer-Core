using System.Collections.Generic;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation.Declaration;
using Orchard;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public interface ISimpleMemoryComponentBuilder : IDependency
    {
        void AddSimpleMemoryComponentsToArchitecture(
            IEnumerable<IArchitectureComponent> invokingComponents,
            Architecture architecture);
    }
}
