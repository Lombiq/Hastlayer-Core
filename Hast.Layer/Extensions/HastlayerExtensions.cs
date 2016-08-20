using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;

namespace Hast.Layer
{
    public static class HastlayerExtensions
    {
        /// <summary>
        /// Generates a proxy for the given object that will transfer suitable calls to the hardware implementation using the default proxy generation configuration.
        /// </summary>
        /// <typeparam name="T">Type of the object to generate a proxy for.</typeparam>
        /// <param name="hardwareRepresentation">The representation of the assemblies implemented as hardware.</param>
        /// <param name="hardwareObject">The object to generate the proxy for.</param>
        /// <returns>The generated proxy object.</returns>
        /// <exception cref="HastlayerException">
        /// Thrown if any lower-level exception or other error happens during proxy generation.
        /// </exception>
        public static Task<T> GenerateProxy<T>(this IHastlayer hastlayer, IHardwareRepresentation hardwareRepresentation, T hardwareObject) where T : class
        {
            return hastlayer.GenerateProxy(hardwareRepresentation, hardwareObject, ProxyGenerationConfiguration.Default);
        }
    }
}
