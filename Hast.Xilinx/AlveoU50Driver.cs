using Hast.Synthesis;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions.ManifestProviders;

namespace Hast.Xilinx
{
    public class AlveoU50Driver : AlveoU50ManifestProvider, IDeviceDriver
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
                    _timingReport ??= _timingReportParser.Parse(ResourceHelper.GetTimingReport(nameof(AlveoU50Driver)));

                    return _timingReport;
                }
            }
        }

        public AlveoU50Driver(ITimingReportParser timingReportParser) => _timingReportParser = timingReportParser;
    }
}
