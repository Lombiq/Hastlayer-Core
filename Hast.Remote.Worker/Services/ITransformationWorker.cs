using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Services;

// This shouldn't be an IDependency because then it would be auto-registered in funny ways when the app is published and
// all DLLs are in a single folder.

/// <summary>
/// This service performs code transformation on the worker server.
/// </summary>
public interface ITransformationWorker
{
    /// <summary>
    /// Transforms every available job in the Azure Blob Storage sequentially. If none are available it waits one second
    /// and then retries. It can only be terminated with the <paramref name="cancellationToken"/>.
    /// </summary>
    Task WorkAsync(CancellationToken cancellationToken);
}
