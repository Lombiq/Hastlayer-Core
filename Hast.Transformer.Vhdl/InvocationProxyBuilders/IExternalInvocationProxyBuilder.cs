using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.ArchitectureComponents;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Transformer.Vhdl.InvocationProxyBuilders
{
    public interface IExternalInvocationProxyBuilder : IDependency
    {
        IArchitectureComponent BuildProxy(
            IEnumerable<IMemberTransformerResult> hardwareEntryPointMemberResults,
            MemberIdTable memberIdTable);
    }
}
