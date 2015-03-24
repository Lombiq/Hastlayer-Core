using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common;
using Hast.Synthesis;

namespace Hast.Xilinx
{
    public class XilinxHardwareRepresentationComposer : IHardwareRepresentationComposer
    {
        public Task Compose(IHardwareDescription hardwareDescription)
        {
            // Not yet implemented.
            return Task.Delay(1);
        }
    }
}
