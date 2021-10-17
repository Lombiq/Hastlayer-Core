using Hast.Layer;
using Microsoft.Extensions.Configuration;
using System;
using static Hast.Remote.Worker.Constants.ConfigurationKeys;

namespace Hast.Remote.Worker.Configuration
{
    public class TransformationWorkerConfiguration : ITransformationWorkerConfiguration
    {
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Returns a configuration object that's automatically configured from the sources in <see
        /// cref="Hastlayer.BuildConfiguration"/>.
        /// </summary>
        public static TransformationWorkerConfiguration Create()
        {
            var appSettings = Hastlayer.BuildConfiguration();

            var connectionString = appSettings.GetConnectionString(StorageConnectionStringKey);
            if (connectionString?.Contains("insert your instrumentation", StringComparison.OrdinalIgnoreCase) == true)
            {
                connectionString = null;
            }

            return new TransformationWorkerConfiguration
            {
                StorageConnectionString =
                    connectionString ??
                    appSettings.GetSection("STORAGE_CONNECTIONSTRING").Value ??
                    "UseDevelopmentStorage=true",
            };
        }
    }
}
