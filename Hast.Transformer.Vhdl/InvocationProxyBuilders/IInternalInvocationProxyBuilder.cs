using System.Collections.Generic;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public interface IInternalInvocationProxyBuilder : IDependency
    {
        IEnumerable<IArchitectureComponent> BuildProxy(
            IEnumerable<IArchitectureComponent> components,
            IVhdlTransformationContext transformationContext);
    }
}
