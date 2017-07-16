using System.Threading;
using System.Threading.Tasks;
using Hast.Remote.Worker.Configuration;
using Orchard;

namespace Hast.Remote.Worker
{
    public interface ITransformationWorker : ISingletonDependency
    {
        Task Work(ITransformationWorkerConfiguration configuration, CancellationToken cancellationToken);
    }
}
