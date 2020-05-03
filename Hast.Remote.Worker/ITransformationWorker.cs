using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker
{
    // This shouldn't be an IDependency because then it would be auto-registered in funny ways when the app is published
    // and all DLLs are in a single folder.
    public interface ITransformationWorker
    {
        Task Work(CancellationToken cancellationToken);
    }
}
