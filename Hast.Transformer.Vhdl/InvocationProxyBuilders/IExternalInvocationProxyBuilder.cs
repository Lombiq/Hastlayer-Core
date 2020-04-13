using System.Collections.Generic;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public interface IExternalInvocationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(
            IEnumerable<IMemberTransformerResult> hardwareEntryPointMemberResults, 
            MemberIdTable memberIdTable);
    }
}
