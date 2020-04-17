using System.Threading;
using System.Threading.Tasks;
using Hast.Common.Interfaces;
using Hast.Remote.Worker.Configuration;

namespace Hast.Remote.Worker
{
    public interface ITransformationWorker : ISingletonDependency
    {
        Task Work(CancellationToken cancellationToken);
    }
}
