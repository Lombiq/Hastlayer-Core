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
                StorageConnectionString = "UseDevelopmentStorage=true"
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

            await host.RunAsync<ITransformationWorker>(worker => worker.Work(CancellationToken.None));
        }
    }
}
