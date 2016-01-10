using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Models
{
    /// <summary>
    /// Carries debug and runtime information about a hardware execution.
    /// </summary>
    public interface IHardwareExecutionInformation
    {
        /// <summary>
        /// The execution time received from the FPGA board.
        /// </summary>
        long FpgaExecutionTime { get; }

        /// <summary>
        /// The full execution time of the hardware execution, including the communication roundtrip.
        /// </summary>
        long FullExecutionTimeMilliseconds { get; }

        /// <summary>
        /// The date when the execution started.
        /// </summary>
        DateTime StartedUtc { get; }
    }
}
