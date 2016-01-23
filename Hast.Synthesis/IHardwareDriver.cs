using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Synthesis.Models;
using Orchard;

namespace Hast.Synthesis
{
    /// <summary>
    /// Provides FPGA-specific implementations.
    /// </summary>
    public interface IHardwareDriver : IDependency
    {
        IHardwareManifest HardwareManifest { get; }
    }
}
