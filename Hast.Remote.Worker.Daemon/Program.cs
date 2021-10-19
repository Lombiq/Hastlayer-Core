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

        public static async Task<int> Main(string[] args) =>
            (int)await MainAsync(args);

        private static async Task<ExitCode> MainAsync(string[] args)
        {
            // Ensure the service is operating from the correct location.
            Directory.SetCurrentDirectory(ApplicationDirectory);

            IInstaller installer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new WindowsInstaller()
                : null; // No Linux/systemd installer as of now, because it's poorly documented on Microsoft's side.

            if (installer != null && args.Length >= 2 && args[0].ToUpperInvariant() == "CLI")
            {
                switch (args[1].ToUpperInvariant())
                {
                    case "INSTALL":
                        return await installer.InstallAsync();
                    case "START":
                        return await installer.StartAsync();
                    case "STOP":
                        return await installer.StopAsync();
                    case "UNINSTALL":
                        return await installer.UninstallAsync();
                }
            }

            try
            {
                await Host.CreateDefaultBuilder(args)
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
                    .RunAsync();
            }
            catch (OperationCanceledException)
            {
                // Nothing to do here.
            }
            catch (Exception exception)
            {
                NoDependencyFatalErrorLogger.Log(exception);
                return ExitCode.StartupException;
            }

            return ExitCode;
        }
    }
}
