using Microsoft.Azure.Storage.Blob;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker
{
    /// <summary>
    /// Retrieves transformation jobs from the cloud and performs them, while also running telemetry.
    /// </summary>
    /// <remarks>
    /// <para>This shouldn't be an IDependency because then it would be auto-registered in funny ways when the app is
    /// published and all DLLs are in a single folder.</para>
    /// </remarks>
    public interface ITransformationWorker
    {
        /// <summary>
        /// Performs all available transformation jobs in the <see cref="CloudBlobContainer"/>.
        /// </summary>
        Task WorkAsync(CancellationToken cancellationToken = default);
    }
}
