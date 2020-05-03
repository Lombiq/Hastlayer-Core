using Hast.Common.Interfaces;
using System;

namespace Hast.Remote.Worker.Services
{
    public interface ITransformationTelemetry
    {
        string JobName { get; }
        int AppId { get; }
        DateTime StartTimeUtc { get; }
        DateTime FinishTimeUtc { get; }
        bool IsSuccess { get; }
    }


    public interface IApplicationInsightsTelemetryManager : ISingletonDependency
    {
        void TrackTransformation(ITransformationTelemetry telemetry);
    }
}
