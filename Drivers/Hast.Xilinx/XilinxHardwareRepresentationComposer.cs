using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Common.Models;
using Hast.Synthesis;

namespace Hast.Xilinx
{
    public class XilinxHardwareRepresentationComposer : IHardwareImplementationComposer
    {
        public Task Compose(IHardwareRepresentation hardwareRepresentation)
        {
            // Not yet implemented.
            return Task.Delay(1);
        }
    }
}
