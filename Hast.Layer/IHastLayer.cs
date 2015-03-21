using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Configuration;
using Orchard;

namespace Hast.Layer
{
    public delegate void TransformedEventHandler(IHardwareDescription hardwareDescription);


    public interface IHastlayer : IDisposable
    {
        /// <summary>
        /// Occurs when the .NET assembly was transformed to hardware description.
        /// </summary>
        event TransformedEventHandler Transformed;

        /// <summary>
        /// Generates and implements a hardware representation of the given assembly.
        /// </summary>
        /// <param name="assembly">The assembly that should be implemented as hardware.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The representation of the assembly implemented as hardware.</returns>
        Task<IHardwareAssembly> GenerateHardware(Assembly assembly, IHardwareGenerationConfiguration configuration);

        // Maybe this as well?
        //Task<IHardwareAssembly> GenerateHardware(Type type); // Would only transform this type and its dependencies.

        /// <summary>
        /// Generates a proxy for the given object that will transfer suitable calls to the hardware implementation.
        /// </summary>
        /// <typeparam name="T">Type of the object to generate a proxy for.</typeparam>
        /// <param name="hardwareAssembly">The representation of the assembly implemented as hardware.</param>
        /// <param name="hardwareObject">The object to generate the proxy for.</param>
        /// <returns>The generated proxy object.</returns>
        Task<T> GenerateProxy<T>(IHardwareAssembly hardwareAssembly, T hardwareObject) where T : class;
    }
}
