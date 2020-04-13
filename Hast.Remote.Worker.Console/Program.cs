using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Remote.Worker.Configuration;

namespace Hast.Remote.Worker.Console
{
    class Program
    {
        static async Task Main()
        {
            using var host = (Hastlayer)Hastlayer.Create();
            await host.RunAsync<ITransformationWorker>(worker =>
            {
                var configuration = new TransformationWorkerConfiguration
                {
                    StorageConnectionString = "UseDevelopmentStorage=true"
                };

                return worker.Work(configuration, CancellationToken.None);
            });
        }
    }
}
