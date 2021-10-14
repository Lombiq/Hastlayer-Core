using Hast.Remote.Worker.Daemon.Constants;

namespace Hast.Remote.Worker.Daemon.Services
{
    /// <summary>
    /// Creates a system event.
    /// </summary>
    public interface IEventLogger
    {
        /// <summary>
        /// Creates a system event with the text containing <see cref="ServiceProperties.DisplayName"/> and
        /// <paramref name="statusText"/>.
        /// </summary>
        /// <param name="statusText">The new state of the service.</param>
        void UpdateStatus(string statusText);
    }
}
