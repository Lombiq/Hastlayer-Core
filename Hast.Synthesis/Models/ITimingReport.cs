using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Synthesis.Models
{
    /// <summary>
    /// Timing report containing hardware timing information on binary operations.
    /// </summary>
    public interface ITimingReport
    {
        /// <summary>
        /// Retrieves the timing value for the given binary operation.
        /// </summary>
        /// <param name="binaryOperator">The operator of the binary operation.</param>
        /// <param name="operandSizeBits">The size of the operation's operands, in bits.</param>
        /// <param name="isSigned">Indicates whether the operands are signed.</param>
        /// <returns>The latency, in ns, what the operation will roughly take. -1 if no timing data was found.</returns>
        decimal GetLatencyNs(BinaryOperatorType binaryOperator, ushort operandSizeBits, bool isSigned);
    }
}
