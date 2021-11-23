using Hast.Catapult;
using Hast.Common.Enums;
using Hast.Common.Services;
using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Hast.Synthesis.Services;
using Hast.Transformer;
using Hast.Transformer.Vhdl.Services;
using Hast.Xilinx;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NLog.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Hast.Remote.Worker.Services
{
    public class HastlayerConfigurationProvider : IHastlayerConfigurationProvider
    {
        private IHastlayerConfiguration _configuration;

        public async Task<IHastlayerConfiguration> GetConfiguration(
            ITransformationWorkerConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            if (_configuration != null) return _configuration;

            var container = CloudStorageAccount
                .Parse(configuration.StorageConnectionString)
                .CreateCloudBlobClient()
                .GetContainerReference("transformation");
            if (!await container.ExistsAsync(cancellationToken))
            {
                await container.CreateAsync(
                    BlobContainerPublicAccessType.Off,
                    options: null,
                    operationContext: null,
                    cancellationToken);
            }

            _configuration = new HastlayerConfiguration
            {
                Flavor = HastlayerFlavor.Developer,
                // These extensions need to be added explicitly because when deployed as a flat folder of binaries they
                // won't be automatically found under the Hast.Core and Hast.Abstractions folders.
                Extensions = new[]
                {
                    typeof(DefaultTransformer).Assembly,
                    typeof(DefaultJsonConverter).Assembly,
                    typeof(VhdlTransformingEngine).Assembly,
                    typeof(NexysA7Driver).Assembly,
                    typeof(TimingReportParser).Assembly,
                    typeof(CatapultDriver).Assembly,
                    typeof(ApplicationInsightsTelemetryManager).Assembly,
                },
                ConfigureLogging = ConfigureLogging,
                OnServiceRegistration = (_, services) =>
                {
                    services.AddSingleton(configuration);
                    services.AddSingleton(container);
                    services.AddSingleton<ITransformationWorker, TransformationWorker>();
                },
            };

            return _configuration;
        }

        public static void ConfigureLogging(ILoggingBuilder builder) =>
            builder
                .AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace)
                .AddApplicationInsights(ApplicationInsightsTelemetryManager.GetInstrumentationKey())
                .AddNLog("NLog.config");
    }
}
