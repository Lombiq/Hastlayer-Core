using Hast.Common.Interfaces;
using Hast.Remote.Worker.Models;
using Microsoft.ApplicationInsights;

namespace Hast.Remote.Worker.Services;

/// <summary>
/// Service for using Azure Application Insights' app telemetry feature.
/// </summary>
public interface IApplicationInsightsTelemetryManager : ISingletonDependency
{
    /// <summary>
    /// Sends a telemetry request to <see cref="TelemetryClient"/> for tracking application resource usage.
    /// </summary>
    /// <param name="telemetry">The details of the request.</param>
    void TrackTransformation(TransformationTelemetry telemetry);
}
