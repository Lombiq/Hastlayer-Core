using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.Common.Interfaces;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public interface ISimpleMemoryComponentBuilder : IDependency
    {
        void AddSimpleMemoryComponentsToArchitecture(
            IEnumerable<IArchitectureComponent> invokingComponents,
            Architecture architecture);
    }
}
