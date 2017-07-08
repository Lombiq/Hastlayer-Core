using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Orchard;

namespace Hast.Transformer.Abstractions
{
    /// <summary>
    /// Service for transforming a .NET assembly into hardware description.
    /// </summary>
    public interface ITransformer : IDependency
    {
        /// <summary>
        /// Transforms the given assembly to hardware description.
        /// </summary>
        /// <param name="assemblyPaths">The file path to the assemblies to transform.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The hardware description created from the assemblies.</returns>
        Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration);

        /// <summary>
        /// Transforms the given assembly to hardware description.
        /// </summary>
        /// <param name="assemblies">The assemblies to transform.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The hardware description created from the assemblies.</returns>
        Task<IHardwareDescription> Transform(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration);
    }
}
