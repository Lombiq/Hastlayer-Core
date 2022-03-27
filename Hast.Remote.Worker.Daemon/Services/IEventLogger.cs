using Hast.Layer;
using Hast.Remote.Worker.Daemon.Constants;
using Microsoft.Extensions.Logging;

namespace Hast.Remote.Worker.Daemon.Services;

/// <summary>
/// Creates a system event.
/// </summary>
public interface IEventLogger
{
    /// <summary>
    /// Gets or sets the logger instance for internal status updates. This is a property so it can bu updated by the
    /// inner <see cref="Hastlayer"/> scope.
    /// </summary>
    ILogger Logger { get; set; }

    /// <summary>
    /// Creates a system event with the text containing <see cref="ServiceProperties.DisplayName"/> and
    /// <paramref name="statusText"/>.
    /// </summary>
    /// <param name="statusText">The new state of the service.</param>
    void UpdateStatus(string statusText);
}
