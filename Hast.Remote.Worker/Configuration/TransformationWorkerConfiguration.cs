using Hast.Layer;
using Hast.Remote.Worker.Constants;
using System;

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
            var configuration = Hastlayer.BuildConfiguration();

            var connectionString = configuration.GetSection(ConfigurationPaths.StorageConnectionString).Value;
            if (connectionString?.Contains("insert your instrumentation", StringComparison.OrdinalIgnoreCase) == true)
            {
                connectionString = null;
            }

            return new TransformationWorkerConfiguration
            {
                StorageConnectionString =
                    connectionString ??
                    configuration.GetSection("STORAGE_CONNECTIONSTRING").Value ??
                    "UseDevelopmentStorage=true",
            };
        }
    }
}
