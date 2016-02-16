using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Orchard;

namespace Hast.Transformer.Vhdl.InvokationProxyBuilders
{
    public class SimpleMemoryOperationProxyBuilder : ISimpleMemoryOperationProxyBuilder
    {
        public IArchitectureComponent BuildProxy(IEnumerable<IArchitectureComponent> components)
        {
            foreach (var component in components.Where(c => c.AreSimpleMemorySignalsAdded()))
            {

            }

            throw new NotImplementedException();
        }
    }
}
