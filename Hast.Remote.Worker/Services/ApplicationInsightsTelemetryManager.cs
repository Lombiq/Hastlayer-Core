using Hast.Common.Interfaces;
using Hast.Layer;
using Hast.Remote.Worker.Exceptions;
using Hast.Remote.Worker.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;

namespace Hast.Remote.Worker.Services
{
    [DependencyInitializer(nameof(InitializeService))]
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

        public void TrackTransformation(TransformationTelemetry telemetry)
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
                throw new MissingInstrumentationKeyException(
                    "Please set up the instrumentation key via appsettings.json or environment variable, see " +
                    "APPINSIGHTS_INSTRUMENTATIONKEY part here: https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core");
            }

            return key;
        }

        public static void InitializeService(IServiceCollection services)
        {
            string key = null;
            try
            {
                key = GetInstrumentationKey();
            }
            catch (MissingInstrumentationKeyException ex)
            {
                // It's not guaranteed that we'd actually use it at this point and a lack of telemetry is not the kind
                // of issue that warrants crashing the application.
                services.LogDeferred(LogLevel.Warning, ex.Message);
            }

            if (key == null) return;

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
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>((_, _) => { });

            // Dependency tracking is disabled as it's not useful (only records blob storage operations) but causes
            // excessive AI usage.
            var dependencyTrackingTelemetryModule = services
                .FirstOrDefault(t => t.ImplementationType == typeof(DependencyTrackingTelemetryModule));
            if (dependencyTrackingTelemetryModule != null) services.Remove(dependencyTrackingTelemetryModule);
        }
    }
}
