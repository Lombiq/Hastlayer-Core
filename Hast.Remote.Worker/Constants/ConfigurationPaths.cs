using Hast.Remote.Worker.Services;

namespace Hast.Remote.Worker.Constants;

public static class ConfigurationPaths
{
    /// <summary>
    /// Configuration key for the Azure Blob Storage connection string where jobs for the Worker are saved and where the
    /// Worker uploads results.
    /// </summary>
    public const string StorageConnectionString = "Hast:Remote_Worker:Storage_ConnectionString";

    /// <summary>
    /// The appsettings path for the Application Insights Instrumentation key. Needed to set up the <see
    /// cref="ApplicationInsightsTelemetryManager"/>.
    /// </summary>
    public const string ApplicationInsightsInstrumentationKey = "Hast:Remote_Worker:ApplicationInsights_InstrumentationKey";
}
