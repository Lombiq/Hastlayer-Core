using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Layer.Extensibility.Events;
using Orchard;

namespace Hast.Layer
{
    public interface IHastlayer : IDisposable
    {
        /// <summary>
        /// Occurs when the member invokation (e.g. a method call) was transferred to hardware and finished there.
        /// </summary>
        event ExecutedOnHardwareEventHandler ExecutedOnHardware;

        /// <summary>
        /// Generates and implements a hardware representation of the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies that should be implemented as hardware.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The representation of the assemblies implemented as hardware.</returns>
        /// <exception cref="HastlayerException">
        /// Thrown if any lower-level exception or other error happens during hardware generation.
        /// </exception>
        Task<IHardwareRepresentation> GenerateHardware(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration);

        /// <summary>
        /// Generates a proxy for the given object that will transfer suitable calls to the hardware implementation.
        /// </summary>
        /// <typeparam name="T">Type of the object to generate a proxy for.</typeparam>
        /// <param name="hardwareRepresentation">The representation of the assemblies implemented as hardware.</param>
        /// <param name="hardwareObject">The object to generate the proxy for.</param>
        /// <returns>The generated proxy object.</returns>
        /// <exception cref="HastlayerException">
        /// Thrown if any lower-level exception or other error happens during proxy generation.
        /// </exception>
        Task<T> GenerateProxy<T>(IHardwareRepresentation hardwareRepresentation, T hardwareObject) where T : class;
    }
}
