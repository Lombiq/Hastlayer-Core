﻿namespace Hast.Communication.Extensibility.Pipeline
{
    public interface IMemberInvocationPipelineStepContext : IMemberInvocationContext
    {
        /// <summary>
        /// Indicates whether running the logic on hardware was cancelled to resume member invocation in software.
        /// </summary>
        bool HardwareExecutionIsCancelled { get; }
    }
}
