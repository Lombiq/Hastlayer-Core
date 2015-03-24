using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Layer
{
    internal class HardwareRepresentation : IHardwareRepresentation
    {
        public IEnumerable<Assembly> SoftAssemblies { get; set; }
    }
}
