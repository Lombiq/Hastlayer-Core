using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Remote.Worker.Services
{
    public interface ITransformationTelemetry
    {
        string JobName { get; }
        int UserId { get; }
        DateTime StartTimeUtc { get; }
        DateTime FinishTimeUtc { get;}
        bool IsSuccess { get; }
    }


    public interface IApplicationInsightsTelemetryManager : ISingletonDependency
    {
        void Setup();
        void TrackTransformation(ITransformationTelemetry telemetry);
    }
}
