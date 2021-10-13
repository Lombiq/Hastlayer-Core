using Hast.Remote.Worker.Daemon.Constants;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static Hast.Remote.Worker.Daemon.Constants.ServiceProperties;

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

    public class EventLogger : IEventLogger
    {
        private readonly EventLog _eventLog = new()
        {
            Log = DisplayName,
            // The EventLog source can't contain dots like the service's technical name.
            Source = Name.Replace(".", string.Empty),
        };

        public void UpdateStatus(string statusText)
        {
            if (!EventLog.Exists(_eventLog.Log))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(_eventLog.Source, _eventLog.Log));
            }

            _eventLog.WriteEntry($"{DisplayName} {statusText}.");
        }
    }
}
