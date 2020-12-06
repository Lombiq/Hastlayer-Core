namespace Hast.Remote.Worker.Configuration
{
    /// <summary>
    /// Configuration for <see cref="TransformationWorker"/>.
    /// </summary>
    public interface ITransformationWorkerConfiguration
    {
        /// <summary>
        /// Gets the connection string used in Azure cloud storage.
        /// </summary>
        string StorageConnectionString { get; }
    }
}
