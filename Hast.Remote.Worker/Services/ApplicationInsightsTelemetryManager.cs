using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.Log4NetAppender;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using System;
using System.Reflection;

namespace Hast.Remote.Worker.Services
{
    public class ApplicationInsightsTelemetryManager : IApplicationInsightsTelemetryManager
    {
        private bool _wasSetup;

        public void Setup()
        {
            if (_wasSetup) return;

            // The no argument version of GetRepository is missing from netstandard but it just forwards to GetCallingAssembly.
            var hierarchyRoot = ((Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly())).Root;

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%message"
            };
            patternLayout.ActivateOptions();

            var aiAppender = new ApplicationInsightsAppender
            {
                Name = "ai-appender",
#pragma warning disable CS0618 // Type or member is obsolete
                InstrumentationKey = TelemetryConfiguration.Active.InstrumentationKey,
#pragma warning restore CS0618 // Type or member is obsolete
                Layout = patternLayout
            };
            aiAppender.ActivateOptions();

            hierarchyRoot.AddAppender(aiAppender);

            // This is a hack to use something from the referenced assemblies and thus get them included in the output 
            // directory and be loaded. These are needed for AI.
            _wasSetup = 
                typeof(DependencyTrackingTelemetryModule).Assembly.FullName != null &&
                //typeof(EventAttribute).Assembly.FullName != null &&
                typeof(PerformanceCollectorModule).Assembly.FullName != null &&
                typeof(ServerTelemetryChannel).Assembly.FullName != null;
        }

        public void TrackTransformation(ITransformationTelemetry telemetry)
        {
            var requestTelemetry = new RequestTelemetry
            {
                Name = "transformation",
                Duration = telemetry.FinishTimeUtc - telemetry.StartTimeUtc,
                Timestamp = telemetry.StartTimeUtc,
                Success = telemetry.IsSuccess,
                Url = new Uri(telemetry.JobName, UriKind.Relative)
            };

            requestTelemetry.Context.User.AccountId = telemetry.AppId.ToString();

#pragma warning disable CS0618 // Type or member is obsolete
            new TelemetryClient(TelemetryConfiguration.Active).TrackRequest(requestTelemetry);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
