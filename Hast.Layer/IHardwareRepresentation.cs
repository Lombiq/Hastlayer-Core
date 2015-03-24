using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Layer
{
    /// <summary>
    /// Represents the hardware that was generated from .NET assemblies.
    /// </summary>
    public interface IHardwareRepresentation
    {
        /// <summary>
        /// The original assemblies this hardware assembly was generated from.
        /// </summary>
        IEnumerable<Assembly> SoftAssemblies { get; }
    }
}
