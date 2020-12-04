using System.Collections.Generic;
using Hast.Transformer.Vhdl.Models;
using Hast.VhdlBuilder.Representation;
using Hast.VhdlBuilder.Representation.Expression;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Vhdl.SubTransformers
{
    public interface IStateMachineInvocationBuilder : IDependency
    {
        IBuildInvocationResult BuildInvocation(
            MethodDeclaration targetDeclaration,
            IEnumerable<ITransformedInvocationParameter> transformedParameters,
            int instanceCount,
            ISubTransformerContext context);

        IEnumerable<IVhdlElement> BuildMultiInvocationWait(
            MethodDeclaration targetDeclaration,
            int instanceCount,
            bool waitForAll,
            ISubTransformerContext context);

        IVhdlElement BuildSingleInvocationWait(
            MethodDeclaration targetDeclaration,
            int targetIndex,
            ISubTransformerContext context);
    }

    public interface IBuildInvocationResult
    {
        IEnumerable<Assignment> OutParameterBackAssignments { get; }
    }
}
