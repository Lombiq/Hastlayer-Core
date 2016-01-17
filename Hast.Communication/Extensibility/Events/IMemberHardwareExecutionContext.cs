using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;

namespace Hast.Communication.Extensibility.Events
{
    /// <summary>
    /// The context for a hardware execution of a hardware-implemented member.
    /// </summary>
    public interface IMemberHardwareExecutionContext : IMemberInvocationContext
    {
        /// <summary>
        /// Debug and runtime information about the hardware execution.
        /// </summary>
        IHardwareExecutionInformation ExecutionInformation { get; }
    }
}
