using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Models;

namespace Hast.Layer.Models
{
    internal class HardwareRepresentation : IHardwareRepresentation
    {
        public IEnumerable<Assembly> SoftAssemblies { get; set; }
        public IHardwareDescription HardwareDescription { get; set; }
    }
}
