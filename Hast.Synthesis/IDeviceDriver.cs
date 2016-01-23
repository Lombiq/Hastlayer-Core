using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Synthesis.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard;

namespace Hast.Synthesis
{
    /// <summary>
    /// Provides FPGA-specific implementations.
    /// </summary>
    public interface IDeviceDriver : IDependency
    {
        IDeviceManifest DeviceManifest { get; }

        uint GetClockCyclesNeededForOperation(BinaryOperatorType operation);
    }
}
