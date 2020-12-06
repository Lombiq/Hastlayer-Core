using Hast.Common.Interfaces;
using Hast.Layer;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Hast.Remote.Worker.Services
{
    [IDependencyInitializer(nameof(InitializeService))]
    public class ApplicationInsightsTelemetryManager : IApplicationInsightsTelemetryManager
    {
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
                Url = new Uri(telemetry.JobName, UriKind.Relative),
            };

            requestTelemetry.Context.User.AccountId = telemetry.AppId.ToString(CultureInfo.InvariantCulture);

            _telemetryClient.TrackRequest(requestTelemetry);
        }

        public static string GetInstrumentationKey()
        {
            var configuration = Hastlayer.BuildConfiguration();
            var key = configuration.GetSection("ApplicationInsights").GetSection("InstrumentationKey").Value ??
                configuration.GetSection("APPINSIGHTS_INSTRUMENTATIONKEY").Value;
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Please set up the instrumentation key via appsettings.json or environment " +
                    "variable, see APPINSIGHTS_INSTRUMENTATIONKEY part here: https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core");
            }

            return key;
        }

        public static void InitializeService(IServiceCollection services)
        {
            var key = GetInstrumentationKey();
            var options = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                InstrumentationKey = key,
                EnableDebugLogger = true,
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
    }
}
