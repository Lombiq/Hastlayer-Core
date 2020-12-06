using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Console
{
    internal class Program
    {
        private static async Task Main()
        {
            var configuration = new TransformationWorkerConfiguration
            {
                StorageConnectionString = "UseDevelopmentStorage=true",
            };

            using var host = (Hastlayer)await TransformationWorker.CreateHastlayerAsync(configuration);

#if DEBUG
            var logger = host.GetLogger<Program>();
            for (int i = 0; i < (int)LogLevel.None; i++)
            {
                var logLevel = (LogLevel)i;
                logger.Log(logLevel, $"{logLevel} testing");
            }
#endif

            var cancellationTokenSource = new CancellationTokenSource();
            System.Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                System.Console.WriteLine("Application cancelled via SIGINT, attempting graceful shutdown. Please allow at least 10 seconds for this...");
                cancellationTokenSource.Cancel();
            };
            System.Console.WriteLine("Press Ctrl + C to cleanly terminate the application.");

            await host.RunAsync<ITransformationWorker>(worker => worker.WorkAsync(cancellationTokenSource.Token));
        }
    }
}
