using Hast.Common.Interfaces;
using Hast.Layer;
using log4net;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.Log4NetAppender;
using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Linq;
using System.Reflection;

namespace Hast.Remote.Worker.Services
{
    [IDependencyInitializer(nameof(InitializeService))]
    public class ApplicationInsightsTelemetryManager : IApplicationInsightsTelemetryManager
    {
        private bool _wasSetup;

        private readonly ILogger<ApplicationInsightsTelemetryManager> _logger;
        private readonly TelemetryClient _telemetryClient;

        public ApplicationInsightsTelemetryManager(
            ILogger<ApplicationInsightsTelemetryManager> logger,
            TelemetryConfiguration telemetryConfiguration,
            TelemetryClient telemetryClient)
        {
            var builder = telemetryConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;
            builder.UseAdaptiveSampling(maxTelemetryItemsPerSecond: 5, excludedTypes: "Event");
            builder.Build();

            _logger = logger;
            _telemetryClient = telemetryClient;
        }

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
                InstrumentationKey = _telemetryClient.InstrumentationKey,
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

            _telemetryClient.TrackRequest(requestTelemetry);
        }

        public static void InitializeService(IServiceCollection services)
        {
            var key = Hastlayer.BuildConfiguration().GetSection("ApplicationInsights").GetSection("InstrumentationKey").Value ??
                Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY") ??
                Environment.GetEnvironmentVariable("ApplicationInsights:InstrumentationKey");
            if (string.IsNullOrEmpty(key))
            {
                throw new Exception("Please set up the instrumentation key via appsettings.json or environment " +
                    "variable, see APPINSIGHTS_INSTRUMENTATIONKEY part here: https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core");
            }
            var options = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                InstrumentationKey = key,
            };

            services.AddLogging(loggingBuilder => loggingBuilder
                .AddFilter<ApplicationInsightsLoggerProvider>("Category", LogLevel.Information));
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
