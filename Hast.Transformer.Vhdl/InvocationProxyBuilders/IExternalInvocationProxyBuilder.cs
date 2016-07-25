using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
