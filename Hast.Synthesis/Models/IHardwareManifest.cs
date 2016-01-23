using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Synthesis.Models
{
    /// <summary>
    /// Describes the capabilities of the connected FPGA.
    /// </summary>
    public interface IHardwareManifest
    {
        /// <summary>
        /// The clock frequency of the board in Hz.
        /// </summary>
        uint ClockFrequencyHz { get; }
    }
}
