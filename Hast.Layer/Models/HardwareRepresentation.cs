﻿using System.Collections.Generic;
using System.Reflection;

namespace Hast.Layer.Models
{
    internal class HardwareRepresentation : IHardwareRepresentation
    {
        public IEnumerable<Assembly> SoftAssemblies { get; set; }
        public IHardwareDescription HardwareDescription { get; set; }
        public IHardwareImplementation HardwareImplementation { get; set; }
        public IDeviceManifest DeviceManifest { get; set; }
    }
}
