using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Layer
{
    /// <summary>
    /// Represents a .NET assembly that was transformed to and is implemented as hardware.
    /// </summary>
    public interface IHardwareAssembly
    {
        /// <summary>
        /// The original assembly this hardware assembly was generated from.
        /// </summary>
        Assembly SoftAssembly { get; }
    }
}
