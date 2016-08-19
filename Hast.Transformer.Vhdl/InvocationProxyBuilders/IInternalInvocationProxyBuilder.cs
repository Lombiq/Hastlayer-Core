using System.Collections.Generic;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Orchard;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public interface IInternalInvocationProxyBuilder : IDependency
    {
        IEnumerable<IArchitectureComponent> BuildProxy(
            IEnumerable<IArchitectureComponent> components,
            IVhdlTransformationContext transformationContext);
    }
}
