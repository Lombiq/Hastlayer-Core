using Hast.Remote.Worker.Services;

namespace Hast.Remote.Worker.Configuration;

/// <summary>
/// Configuration for <see cref="TransformationWorker"/>.
/// </summary>
public interface ITransformationWorkerConfiguration
{
    /// <summary>
    /// Gets the connection string used in Azure Blob Storage.
    /// </summary>
    string StorageConnectionString { get; }
}
