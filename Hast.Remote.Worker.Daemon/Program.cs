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

namespace Hast.Remote.Worker.Daemon;

public static class Program
{
    public static string ApplicationDirectory { get; } = AppContext.BaseDirectory;

    public static ExitCode ExitCode { get; set; } = ExitCode.Success;

    public static async Task<int> Main(string[] args) =>
        (int)await MainAsync(args);

    private static async Task<ExitCode> MainAsync(string[] args)
    {
        // Ensure the service is operating from the correct location. Normally services are started from the home
        // path of the service user as the working directory. For example on Windows services start in either
        // %WinDir%\System32 or %WinDir%\SysWOW64.
        Directory.SetCurrentDirectory(ApplicationDirectory);

        IInstaller installer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new WindowsInstaller()
            : null; // No Linux/systemd installer as of now, because it's poorly documented on Microsoft's side.

        if (installer != null && args.Length >= 2 && args[0].ToUpperInvariant() == "CLI")
        {
            return args[1].ToUpperInvariant() switch
            {
                "INSTALL" => await installer.InstallAsync(),
                "START" => await installer.StartAsync(),
                "STOP" => await installer.StopAsync(),
                "UNINSTALL" => await installer.UninstallAsync(),
                _ => throw new ArgumentOutOfRangeException(nameof(args), $"Unknown value \"{args[1]}\"."),
            };
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
