using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions;

namespace Hast.Xilinx
{
    public class NexysA7Driver : NexysDriverBase
    {
        static NexysA7Driver() => DeviceNameInternals[nameof(NexysA7Driver)] = NexysA7ManifestProvider.DeviceName;

        public NexysA7Driver(ITimingReportParser timingReportParser) : base(timingReportParser)
        {
        }
    }
}
