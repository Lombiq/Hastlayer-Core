using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Synthesis.Models
{
    /// <summary>
    /// Describes the capabilities of the connected hardware device.
    /// </summary>
    public interface IDeviceManifest
    {
        /// <summary>
        /// Technical name that identifies the device.
        /// </summary>
        string TechnicalName { get; }

        /// <summary>
        /// The clock frequency of the board in Hz.
        /// </summary>
        uint ClockFrequencyHz { get; }

        /// <summary>
        /// Gets the names of those communication channels usable with the connected device. The first one will be used
        /// as the default.
        /// </summary>
        IEnumerable<string> SupportedCommunicationChannelNames { get; }
    }
}
