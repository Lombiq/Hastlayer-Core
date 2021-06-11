using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public interface IInternalInvocationProxyBuilder : IDependency
    {
        IEnumerable<IArchitectureComponent> BuildProxy(
            ICollection<IArchitectureComponent> components,
            IVhdlTransformationContext transformationContext);
    }
}
