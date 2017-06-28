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
    /// Converts assignments where there is an operator other than equality to simple assignments.
    /// </summary>
    /// <example>
    /// array[4] += 10;
    /// 
    /// ...will be converted to:
    /// 
    /// array[4] = array[4] + 10;
    /// </example>
    /// <remarks>
    /// Supposedly only array access should remain in such forms, everything else should be simple assignments.
    /// </remarks>
    public interface IOperatorAssignmentsToSimpleAssignmentsConverter : IDependency
    {
        void ConvertOperatorAssignmentExpressionsToSimpleAssignments(SyntaxTree syntaxTree);
    }
}
