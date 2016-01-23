using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Models;
using Hast.Synthesis;
using Hast.Synthesis.Models;

namespace Hast.Xilinx
{
    public class VivadoHardwareImplementationComposer : IHardwareImplementationComposer
    {
        public Task<IHardwareImplementation> Compose(IHardwareDescription hardwareDescription)
        {
            // Not yet implemented.
            return Task.FromResult((IHardwareImplementation)new HardwareImplementation());
        }
    }
}
