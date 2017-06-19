using System.Collections.Generic;
using System.Reflection;

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

        /// <summary>
        /// Represents a handle to the hardware implementation synthesized through the FPGA vendor toolchain.
        /// </summary>
        IHardwareImplementation HardwareImplementation { get; }

        /// <summary>
        /// Describes the capabilities, like available memory, of the connected hardware device.
        /// </summary>
        IDeviceManifest DeviceManifest { get; }
    }
}
