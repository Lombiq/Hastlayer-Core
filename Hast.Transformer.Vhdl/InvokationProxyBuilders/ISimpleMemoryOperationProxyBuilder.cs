using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Orchard;

namespace Hast.Transformer.Vhdl.InvokationProxyBuilders
{
    public interface ISimpleMemoryOperationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(IEnumerable<IArchitectureComponent> components);
    }
}
