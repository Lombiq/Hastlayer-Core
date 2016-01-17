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
        /// Generates a hardware representation of the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies that should be implemented as hardware.</param>
        /// <param name="configuration">Configuration for how the hardware generation should happen.</param>
        /// <returns>The hardware representation of the assemblies.</returns>
        /// <exception cref="HastlayerException">Thrown if any lower-level exception or other error happens during hardware generation.</exception>
        Task<IHardwareRepresentation> GenerateHardware(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration);

        /// <summary>
        /// Materializes the generated hardware so it can be used for executing implemented algorithms.
        /// </summary>
        /// <param name="hardwareRepresentation">The representation of the assemblies implemented as hardware.</param>
        /// <returns>The handle to the materialized hardware.</returns>
        /// <exception cref="HastlayerException">Thrown if any lower-level exception or other error happens during materializing the hardware.</exception>
        Task<IMaterializedHardware> MaterializeHardware(IHardwareRepresentation hardwareRepresentation);

        /// <summary>
        /// Generates a proxy for the given object that will transfer suitable calls to the hardware implementation.
        /// </summary>
        /// <typeparam name="T">Type of the object to generate a proxy for.</typeparam>
        /// <param name="materializedHardware">The handle to the materialized hardware.</param>
        /// <param name="hardwareObject">The object to generate the proxy for.</param>
        /// <returns>The generated proxy object.</returns>
        /// <exception cref="HastlayerException">Thrown if any lower-level exception or other error happens during proxy generation.</exception>
        Task<T> GenerateProxy<T>(IMaterializedHardware materializedHardware, T hardwareObject) where T : class;
    }
}
