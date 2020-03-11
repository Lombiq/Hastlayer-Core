namespace Hast.Remote.Worker.Daemon
{
    internal static class ConfigurationKeys
    {
        /// <summary>
        /// Configuration key for the Azure Blob Storage connection string where jobs for the Worker are saved and where
        /// the Worker uploads results.
        /// </summary>
        public const string StorageConnectionStringKey = "Hast.Remote.Worker.Daemon.StorageConnectionString";
    }
}
