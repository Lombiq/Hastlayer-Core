using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Extensibility.Pipeline;

namespace Hast.Communication.Extensibility.Pipeline
{
    /// <summary>
    /// Pipeline step to change the invokation of hardware-implemented members.
    /// </summary>
    public interface IMemberInvocationPipelineStep : IPipelineStep
    {
        /// <summary>
        /// Determines whether the invokation of the hardware-implemented member can continue as hardware-implemented logic.
        /// </summary>
        /// <param name="invocationContext">The context of the member invocation.</param>
        /// <returns><c>true</c> if the invocation can continue on hardware, <c>false</c> otherwise.</returns>
        bool CanContinueHardwareInvokation(IMemberInvocationPipelineStepContext invocationContext);
    }
}
