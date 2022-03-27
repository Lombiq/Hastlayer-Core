using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Hast.Remote.Worker.Daemon.Constants.ServiceProperties;

namespace Hast.Remote.Worker.Daemon.Services;

public class EventLogger : IEventLogger
{
    public ILogger Logger { get; set; }

    private readonly EventLog _eventLog;

    public EventLogger(ILogger<EventLogger> logger)
    {
        Logger = logger;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _eventLog = new()
            {
                Log = DisplayName,
                // The EventLog source can't contain dots like the service's technical name.
                Source = Name.Replace(".", string.Empty),
            };
        }
    }

    public void UpdateStatus(string statusText)
    {
        Logger.LogInformation("{0}: {1}", DisplayName, statusText);
        if (_eventLog == null) return;

        // Platform compatibility is already validated by _eventLog not being null at this point.
#pragma warning disable CA1416 // Validate platform compatibility
        if (!EventLog.Exists(_eventLog.Log))
        {
            EventLog.CreateEventSource(new EventSourceCreationData(_eventLog.Source, _eventLog.Log));
        }

        _eventLog.WriteEntry($"{DisplayName} {statusText}.");
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
