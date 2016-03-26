using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
