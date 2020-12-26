using Hast.Synthesis;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions.ManifestProviders;

namespace Hast.Xilinx
{
    public class AlveoU250Driver : AlveoU250ManifestProvider, IDeviceDriver
    {
        private readonly ITimingReportParser _timingReportParser;
        private readonly object _timingReportParserLock = new object();

        private ITimingReport _timingReport;
        public ITimingReport TimingReport
        {
            get
            {
                lock (_timingReportParserLock)
                {
                    _timingReport ??= _timingReportParser.Parse(ResourceHelper.GetTimingReport(nameof(AlveoU250Driver)));

                    return _timingReport;
                }
            }
        }

        public AlveoU250Driver(ITimingReportParser timingReportParser) => _timingReportParser = timingReportParser;
    }
}
