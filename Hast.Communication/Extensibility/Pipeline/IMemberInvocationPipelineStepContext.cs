using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Extensibility.Pipeline
{
    public interface IMemberInvocationPipelineStepContext : IMemberInvocationContext
    {
        /// <summary>
        /// Indicates whether running the logic on hardware was cancelled to resume member invokation in software.
        /// </summary>
        bool HardwareExecutionIsCancelled { get; }
    }
}
