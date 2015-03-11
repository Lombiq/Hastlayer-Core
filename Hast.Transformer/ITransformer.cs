using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Orchard;

namespace Hast.Transformer
{
    /// <summary>
    /// Service for transforming a .NET assembly into hardware description.
    /// </summary>
    public interface ITransformer : IDependency
    {
        /// <summary>
        /// Transforms the given assembly to hardware description.
        /// </summary>
        /// <param name="assemblyPath">The file path to the assembly to transform.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The hardware description created from the assembly.</returns>
        Task<IHardwareDescription> Transform(string assemblyPath, IHardwareGenerationConfiguration configuration);

        /// <summary>
        /// Transforms the given assembly to hardware description.
        /// </summary>
        /// <param name="assembly">The assembly to transform.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The hardware description created from the assembly.</returns>
        Task<IHardwareDescription> Transform(Assembly assembly, IHardwareGenerationConfiguration configuration);
    }
}
