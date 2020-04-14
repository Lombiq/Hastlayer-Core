using Hast.Common.Interfaces;
using Hast.Layer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.NLogTarget;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NLog.Config;
using NLog.Extensions.Logging;
using System;
using System.Linq;


namespace Hast.Remote.Worker.Services
{
    [IDependencyInitializer(nameof(InitializeService))]
    public class ApplicationInsightsTelemetryManager : IApplicationInsightsTelemetryManager
    {
        public static string InstrumentationKey;

        private readonly TelemetryClient _telemetryClient;


        public ApplicationInsightsTelemetryManager(
            TelemetryConfiguration telemetryConfiguration,
            TelemetryClient telemetryClient)
        {
            var builder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
            builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5, excludedTypes: "Event");
            builder.Build();

            _telemetryClient = telemetryClient;
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

            _telemetryClient.TrackRequest(requestTelemetry);
        }

        public static void InitializeService(IServiceCollection services)
        {
            var configuration = Hastlayer.BuildConfiguration();
            InstrumentationKey = configuration.GetSection("ApplicationInsights").GetSection("InstrumentationKey").Value ??
                configuration.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY").Value;
            if (string.IsNullOrEmpty(InstrumentationKey))
            {
                throw new Exception("Please set up the instrumentation key via appsettings.json or environment " +
                    "variable, see APPINSIGHTS_INSTRUMENTATIONKEY part here: https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core");
            }
            var options = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                InstrumentationKey = InstrumentationKey,
            };

            services.AddApplicationInsightsTelemetryWorkerService(options);

            services.AddSingleton<ITelemetryInitializer, HttpDependenciesParsingTelemetryInitializer>();
            services.AddApplicationInsightsTelemetryProcessor<QuickPulseTelemetryProcessor>();
            services.AddApplicationInsightsTelemetryProcessor<AutocollectedMetricsExtractor>();
            services.AddApplicationInsightsTelemetryProcessor<AdaptiveSamplingTelemetryProcessor>();
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((module, o) => { });

            // Dependency tracking is disabled as it's not useful (only records blob storage operations) but causes
            // excessive AI usage.
            var dependencyTrackingTelemetryModule = services
                .FirstOrDefault(t => t.ImplementationType == typeof(DependencyTrackingTelemetryModule));
            if (dependencyTrackingTelemetryModule != null) services.Remove(dependencyTrackingTelemetryModule);
        }

        public static void AddNLogTarget()
        {
            var logTarget = new ApplicationInsightsTarget { InstrumentationKey = InstrumentationKey };
            NLog.LogManager.Configuration.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logTarget);
        }
    }
}
