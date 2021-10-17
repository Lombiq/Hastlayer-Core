using Hast.Remote.Worker.Services;

namespace Hast.Remote.Worker.Constants
{
    public static class ConfigurationKeys
    {
        /// <summary>
        /// Configuration key for the Azure Blob Storage connection string where jobs for the Worker are saved and where
        /// the Worker uploads results.
        /// </summary>
        public const string StorageConnectionStringKey = "Hast.Remote.Worker.Daemon.StorageConnectionString";

        /// <summary>
        /// The appsettings path for the Application Insights Instrumentation key. Needed to set up the <see
        /// cref="ApplicationInsightsTelemetryManager"/>.
        /// </summary>
        public const string ApplicationInsightsInstrumentationKeyPath = "ApplicationInsights::InstrumentationKey";
    }
}
