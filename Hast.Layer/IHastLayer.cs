using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Layer
{
    // Mockup of the eventual real implementation.
    public interface IHastLayer : IDependency // Maybe IDisposable that would free up caches? Or that would defeat the point of caching in this case?
    {
        // Either this...
        Task<IHardwareAssembly> GenerateHardware(Assembly assembly);
        /*
         * Steps:
         * - Transform into hardware definition through ITransformer.
         * - Save hardware definition for re-use (cache file, stream supplied from the outside).
         * - Synthesize hardware through vendor-specific toolchain and load it onto FPGA, together with the necessary communication 
         *   implementation (currently partially implemented with a call chain table).
         * - Cache hardware implementation to be able to re-load it onto the FPGA later.
         */

        // ...or this:
        Task<IHardwareAssembly> GenerateHardware(Type type); // Would only transform this type and its dependencies.

        // Maybe this should return an IDisposable? E.g. close communication to FPGA, or clean up its configuration here if no other calls for
        // this type of object is alive.
        T GenerateProxy<T>(IHardwareAssembly hardwareAssembly, T hardwareObject);
        /*
         * - Generate dynamic proxy for the given object.
         * - For the type's methods implement: FPGA communication, data transformation.
         */

            //if (hardwareAssembly.SoftAssembly != hardwareType.GetType().Assembly)
            //{
            //    throw new InvalidOperationException("The supplied type is not part of this hardware assembly.");
            //}
    }
}
