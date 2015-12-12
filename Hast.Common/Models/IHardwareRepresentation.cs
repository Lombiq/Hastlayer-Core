using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;

namespace Hast.Common.Models
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

        /// <summary>
        /// Describes the hardware created from a transformed assembly.
        /// </summary>
        IHardwareDescription HardwareDescription { get; }
    }
}
