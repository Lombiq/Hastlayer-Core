using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.ArchitectureComponents;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public interface ISimpleMemoryOperationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(IEnumerable<IArchitectureComponent> components);
    }
}
