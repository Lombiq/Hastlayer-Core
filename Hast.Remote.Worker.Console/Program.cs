using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Console
{
    public class Program
    {
        private readonly ITransformationWorker _transformationWorker;
        private readonly IStringLocalizer T;

        protected Program(IServiceProvider provider)
        {
            _transformationWorker = provider.GetRequiredService<ITransformationWorker>();

            T = provider.GetRequiredService<IStringLocalizer<Program>>();
        }

        private Task RunAsync()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            System.Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                System.Console.WriteLine(
                    T["Application cancelled via SIGINT, attempting graceful shutdown. Please allow at least 10 seconds for this..."]);
                cancellationTokenSource.Cancel();
            };
            System.Console.WriteLine(T["Press Ctrl + C to cleanly terminate the application."]);

            return _transformationWorker.WorkAsync(cancellationTokenSource.Token);
        }

        private static async Task Main()
        {
            var configuration = new TransformationWorkerConfiguration
            {
                StorageConnectionString = "UseDevelopmentStorage=true",
            };

            // Those features are for internal use only.
#pragma warning disable S3215 // "interface" instances should not be cast to concrete types
            using var host = (Hastlayer)await TransformationWorker.CreateHastlayerAsync(configuration);
#pragma warning restore S3215 // "interface" instances should not be cast to concrete types

#if DEBUG
            var logger = host.GetLogger<Program>();
            for (int i = 0; i < (int)LogLevel.None; i++)
            {
                var logLevel = (LogLevel)i;
                logger.Log(logLevel, $"{logLevel} testing");
            }
#endif

            await host.RunGetAsync(async provider =>
            {
                await new Program(provider).RunAsync();
                return true;
            });
        }
    }
}
