using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hast.Layer;
using Hast.Remote.Worker.Configuration;

namespace Hast.Remote.Worker.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(async () =>
            {
                //var settings = new AppHostSettings
                //{
                //    ImportedExtensions = new[] { typeof(Program).Assembly, typeof(ITransformationWorker).Assembly },
                //    DefaultShellFeatureStates = new[]
                //    {
                //            new DefaultShellFeatureState
                //            {
                //                EnabledFeatures = new[]
                //                {
                //                    typeof(Program).Assembly.ShortName(),
                //                    typeof(ITransformationWorker).Assembly.ShortName()
                //                }
                //            }
                //    }
                //};

                using (var host = await Hastlayer.Create())
                {
                    await host.Run<ITransformationWorker>(worker =>
                    {
                        var configuration = new TransformationWorkerConfiguration
                        {
                            StorageConnectionString = "UseDevelopmentStorage=true"
                        };

                        return worker.Work(configuration, CancellationToken.None);
                    });
                }
            }).Wait();
        }
    }
}
