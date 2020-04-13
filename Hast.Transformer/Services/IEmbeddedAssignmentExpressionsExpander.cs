using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Searches for assignment expressions embedded in other expressions and brings them up to their own statements,
    /// allowing easier processing later.
    /// </summary>
    /// <example>
    /// if (skipCount = skipCount - 1u <= 0u)
    /// {
    ///     ...
    /// 
    /// ...will be converted into:
    /// uint assignment;
    /// assignment = skipCount - 1u;
    /// skipCount = assignment;
    /// if (assignment <= 0u)
    /// {
    ///     ...
    /// </example>
    /// <remarks>
    /// The MakeAssignmentExpressions configuration of <see cref="ICSharpCode.Decompiler.DecompilerSettings"/> serves
    /// something similar but that also changes how a decompiled Task.Factory.StartNew() looks like.
    /// </remarks>
    public interface IEmbeddedAssignmentExpressionsExpander : IDependency
    {
        void ExpandEmbeddedAssignmentExpressions(SyntaxTree syntaxTree);
    }
}
