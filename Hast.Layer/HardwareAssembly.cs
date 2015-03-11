using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Layer
{
    internal class HardwareAssembly : IHardwareAssembly
    {
        public Assembly SoftAssembly { get; set; }
    }
}
