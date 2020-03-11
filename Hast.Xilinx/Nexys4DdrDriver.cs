using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions;

namespace Hast.Xilinx
{
    public class Nexys4DdrDriver : NexysDriverBase
    {
        public Nexys4DdrDriver(ITimingReportParser timingReportParser) : base(timingReportParser)
        {
            _deviceName = Nexys4DdrManifestProvider.DeviceName;
        }
    }
}
