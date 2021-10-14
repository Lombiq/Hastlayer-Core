using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static Hast.Remote.Worker.Daemon.Constants.ServiceProperties;

namespace Hast.Remote.Worker.Daemon.Services
{
    public class EventLogger : IEventLogger
    {
        private readonly ILogger _logger;

        private readonly EventLog _eventLog = new()
        {
            Log = DisplayName,
            // The EventLog source can't contain dots like the service's technical name.
            Source = Name.Replace(".", string.Empty),
        };

        public EventLogger(ILogger<EventLogger> logger) => _logger = logger;

        public void UpdateStatus(string statusText)
        {
            if (!EventLog.Exists(_eventLog.Log))
            {
                EventLog.CreateEventSource(new EventSourceCreationData(_eventLog.Source, _eventLog.Log));
            }

            _eventLog.WriteEntry($"{DisplayName} {statusText}.");
            _logger.LogInformation("{0}: {1}", DisplayName, statusText);
        }
    }
}
