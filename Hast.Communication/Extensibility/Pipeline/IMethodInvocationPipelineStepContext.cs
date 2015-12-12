using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Extensibility.Pipeline
{
    public interface IMethodInvocationPipelineStepContext : IMethodInvocationContext
    {
        /// <summary>
        /// Indicates whether running the logic on hardware was cancelled to resume method invokation in software.
        /// </summary>
        bool HardwareInvocationIsCancelled { get; }
    }
}
