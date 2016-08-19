using System.Collections.Generic;
using System.Reflection;
using Hast.Common.Models;

namespace Hast.Layer.Models
{
    internal class HardwareRepresentation : IHardwareRepresentation
    {
        public IEnumerable<Assembly> SoftAssemblies { get; set; }
        public IHardwareDescription HardwareDescription { get; set; }
        public IHardwareImplementation HardwareImplementation { get; set; }
    }
}
