using System.Collections.Generic;
using Hast.Common.Interfaces;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IStateMachineInvocationBuilder : IDependency
    {
        IBuildInvocationResult BuildInvocation(
            MethodDeclaration targetDeclaration,
            IEnumerable<TransformedInvocationParameter> transformedParameters,
            int instanceCount,
            SubTransformerContext context);

        IEnumerable<IVhdlElement> BuildMultiInvocationWait(
            MethodDeclaration targetDeclaration,
            int instanceCount,
            bool waitForAll,
            SubTransformerContext context);

        IVhdlElement BuildSingleInvocationWait(
            MethodDeclaration targetDeclaration,
            int targetIndex,
            SubTransformerContext context);
    }

    public interface IBuildInvocationResult
    {
        IEnumerable<Assignment> OutParameterBackAssignments { get; }
    }
}
