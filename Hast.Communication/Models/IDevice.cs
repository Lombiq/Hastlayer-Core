using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Communication.Models
{
    /// <summary>
    /// Represents a compatible device for hardware exeuction.
    /// </summary>
    public interface IDevice
    {
        /// <summary>
        /// Gets a string that uniquely identifies the given device.
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// Gets metadata associated with the device.
        /// </summary>
        dynamic Metadata { get; }
    }
}
