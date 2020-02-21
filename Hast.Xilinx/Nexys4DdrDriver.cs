using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions;

namespace Hast.Xilinx
{
    public class Nexys4DdrDriver : NexysDriverBase
    {
        static Nexys4DdrDriver() => DeviceNameInternals[nameof(Nexys4DdrDriver)] = Nexys4DdrManifestProvider.DeviceName;

        public Nexys4DdrDriver(ITimingReportParser timingReportParser) : base(timingReportParser)
        {
        }
    }
}
