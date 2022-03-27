using Hast.Remote.Worker.Daemon.Constants;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Daemon.Services;

/// <summary>
/// A service/daemon installer and controller service.
/// </summary>
public interface IInstaller
{
    /// <summary>
    /// Installs the service, then starts it and writes any OS output to the console.
    /// </summary>
    Task<ExitCode> InstallAsync();

    /// <summary>
    /// Starts the service and writes any OS output to the console.
    /// </summary>
    Task<ExitCode> StartAsync();

    /// <summary>
    /// Stops the service and writes any OS output to the console.
    /// </summary>
    Task<ExitCode> StopAsync();

    /// <summary>
    /// Uninstalls the service and writes any OS output to the console.
    /// </summary>
    Task<ExitCode> UninstallAsync();
}
