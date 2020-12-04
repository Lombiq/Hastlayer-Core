using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    /// <summary>
    /// Searches for assignment expressions embedded in other expressions and brings them up to their own statements,
    /// allowing easier processing later.
    /// </summary>
    /// <example>
    /// <code>
    /// if (skipCount = skipCount - 1u &lt;= 0u)
    /// {
    ///     ...
    ///
    /// ...will be converted into:
    /// uint assignment;
    /// assignment = skipCount - 1u;
    /// skipCount = assignment;
    /// if (assignment &lt;= 0u)
    /// {
    ///     ...
    /// </code>
    /// </example>
    /// <remarks>
    /// <para>The MakeAssignmentExpressions configuration of <see cref="ICSharpCode.Decompiler.DecompilerSettings"/> serves
    /// something similar but that also changes how a decompiled Task.Factory.StartNew() looks like.</para>
    /// </remarks>
    public interface IEmbeddedAssignmentExpressionsExpander : IDependency
    {
        void ExpandEmbeddedAssignmentExpressions(SyntaxTree syntaxTree);
    }
}
