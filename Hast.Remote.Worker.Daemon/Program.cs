using Hast.Layer;
using Hast.Remote.Worker.Daemon.Constants;
using Hast.Remote.Worker.Daemon.Helpers;
using Hast.Remote.Worker.Daemon.Services;
using Hast.Remote.Worker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Daemon
{
    public static class Program
    {
        public static string ApplicationDirectory { get; } = Path.GetDirectoryName(typeof(Program).Assembly.Location);

        public static ExitCode ExitCode { get; set; } = ExitCode.Success;

        public static int Main(string[] args)
        {
            try
            {
                Host.CreateDefaultBuilder(args)
                    .UseWindowsService(options => options.ServiceName = ServiceProperties.Name)
                    .ConfigureServices((_, services) =>
                    {
                        services.AddSingleton<IEventLogger, EventLogger>();
                        services.AddHostedService<Services.Worker>();
                        Hastlayer.ConfigureLogging(
                            services,
                            HastlayerConfigurationProvider.ConfigureLogging);
                    })
                    .Build()
                    .Run();
            }
            catch (OperationCanceledException)
            {
                // Nothing to do here.
            }
            catch(Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                ExitCode = ExitCode.StartupException;
            }

            return (int)ExitCode;
        }
    }
}
