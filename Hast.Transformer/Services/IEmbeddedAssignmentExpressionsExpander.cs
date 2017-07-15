using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

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
    /// skipCount = skipCount - 1u;
    /// if (skipCount <= 0u)
    /// {
    ///     ...
    /// </example>
    public interface IEmbeddedAssignmentExpressionsExpander : IDependency
    {
        void ExpandEmbeddedAssignmentExpressions(SyntaxTree syntaxTree);
    }
}
