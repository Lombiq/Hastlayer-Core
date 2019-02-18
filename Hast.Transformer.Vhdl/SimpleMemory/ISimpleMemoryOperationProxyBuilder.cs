using System.Collections.Generic;
using Hast.Transformer.Models;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Orchard;

namespace Hast.Transformer.Vhdl.SimpleMemory
{
    public interface ISimpleMemoryOperationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(
            IEnumerable<IArchitectureComponent> components,
            ITransformationContext transformationContext);
    }
}
