using Hast.Transformer.Vhdl.ArchitectureComponents;
using Orchard;
using System.Collections.Generic;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public interface ISimpleMemoryOperationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(IEnumerable<IArchitectureComponent> components);
    }
}
