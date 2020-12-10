using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation.Declaration;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public interface ISimpleMemoryComponentBuilder : IDependency
    {
        void AddSimpleMemoryComponentsToArchitecture(
            IEnumerable<IArchitectureComponent> invokingComponents,
            Architecture architecture);
    }
}
