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
        /// The net execution time (without the communication roundtrip) on the hardware.
        /// </summary>
        ulong HardwareExecutionTimeMilliseconds { get; }

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
