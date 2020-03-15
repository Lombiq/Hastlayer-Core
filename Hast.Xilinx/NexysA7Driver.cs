using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions.ManifestProviders;

namespace Hast.Xilinx
{
    public class NexysA7Driver : NexysDriverBase
    {
        static NexysA7Driver() => DeviceNameInternal = NexysA7ManifestProvider.DeviceName;

        public NexysA7Driver(ITimingReportParser timingReportParser) : base(timingReportParser)
        {
        }
    }
}
