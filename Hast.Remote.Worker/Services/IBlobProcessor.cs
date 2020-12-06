using Hast.Common.Interfaces;
using Microsoft.Azure.Storage.Blob;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Services
{
    /// <summary>
    /// A service that reads and transforms a serialized assembly retreived from a cloud container.
    /// </summary>
    public interface IBlobProcessor : IDependency
    {
        /// <summary>
        /// It takes a cloud blob, deserializes it, generates hardware representation and then writes it back into the blob.
        /// </summary>
        Task StartJobAsync(CloudBlockBlob blob, CancellationToken cancellationToken = default);
    }
}
