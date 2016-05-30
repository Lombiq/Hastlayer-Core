﻿using System;
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
            ISubTransformerContext context);

        IEnumerable<IVhdlElement> BuildInvocationWait(
            EntityDeclaration targetDeclaration,
            int instanceCount,
            ISubTransformerContext context);
    }
}
