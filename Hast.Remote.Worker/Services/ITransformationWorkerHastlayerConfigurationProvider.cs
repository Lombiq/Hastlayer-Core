using Hast.Layer;
using Hast.Remote.Worker.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace Hast.Remote.Worker.Services
{
    /// <summary>
    /// Provides <see cref="IHastlayerConfiguration"/> for the <see cref="TransformationWorker"/>. This is used outside
    /// of <see cref="Hastlayer"/>'s root dependency injection scope in case it's constructed with the help of an outer
    /// service provider.
    /// </summary>
    public interface ITransformationWorkerHastlayerConfigurationProvider
    {
        /// <summary>
        /// Gets or creates a <see cref="IHastlayerConfiguration"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="TransformationWorker"/>'s configuration.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The existing or new configuration object.</returns>
        Task<IHastlayerConfiguration> GetConfiguration(
            ITransformationWorkerConfiguration configuration,
            CancellationToken cancellationToken = default);
    }
}
