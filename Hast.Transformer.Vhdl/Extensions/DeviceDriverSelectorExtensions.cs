using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Vhdl.Models;

namespace Hast.Synthesis.Services
{
    internal static class DeviceDriverSelectorExtensions
    {
        public static IDeviceDriver GetDriver(this IDeviceDriverSelector deviceDriverSelector, ISubTransformerContext context) =>
            deviceDriverSelector.GetDriver(context.TransformationContext.HardwareGenerationConfiguration.DeviceName);
    }
}
