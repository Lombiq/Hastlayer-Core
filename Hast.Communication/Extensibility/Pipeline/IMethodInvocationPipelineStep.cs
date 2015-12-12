using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Extensibility.Pipeline;

namespace Hast.Communication.Extensibility.Pipeline
{
    /// <summary>
    /// Pipeline step to change the invokation of hardware-implemented methods.
    /// </summary>
    public interface IMethodInvocationPipelineStep : IPipelineStep
    {
        /// <summary>
        /// Determines whether the invokation of the hardware-implemented method can continue as hardware-implemented logic.
        /// </summary>
        /// <param name="invocationContext">The context of the method invocation.</param>
        /// <returns><c>true</c> if the invocation can continue on hardware, <c>false</c> otherwise.</returns>
        bool CanContinueHardwareInvokation(IMethodInvocationPipelineStepContext invocationContext);
    }
}
