using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IStateMachineInvocationBuilder : IDependency
    {
        void BuildInvocation(
            EntityDeclaration targetDeclaration,
            IEnumerable<IVhdlElement> parameters,
            int instanceCount,
            ISubTransformerContext context);

        IEnumerable<IVhdlElement> BuildMultiInvocationWait(
            EntityDeclaration targetDeclaration,
            int instanceCount,
            bool waitForAll,
            ISubTransformerContext context);

        IVhdlElement BuildSingleInvocationWait(
            EntityDeclaration targetDeclaration,
            int targetIndex,
            ISubTransformerContext context);
    }
}
