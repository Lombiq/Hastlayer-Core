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
    public interface IHastLayer : IDisposable
    {
        // Either this...
        Task<IHardwareAssembly> GenerateHardware(Assembly assembly);

        // ...or this:
        //Task<IHardwareAssembly> GenerateHardware(Type type); // Would only transform this type and its dependencies.

        T GenerateProxy<T>(IHardwareAssembly hardwareAssembly, T hardwareObject);
    }
}
