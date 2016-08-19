using System.Collections.Generic;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Orchard;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public interface IExternalInvocationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(
            IEnumerable<IMemberTransformerResult> interfaceMemberResults, 
            MemberIdTable memberIdTable);
    }
}
