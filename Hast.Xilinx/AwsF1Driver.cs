using Hast.Synthesis;
using Hast.Synthesis.Helpers;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions.ManifestProviders;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Xilinx
{
    public class AwsF1Driver : AwsF1ManifestProvider, IDeviceDriver
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
                    _timingReport ??= _timingReportParser.Parse(ResourceHelper.GetTimingReport(nameof(AwsF1Driver)));

                    return _timingReport;
                }
            }
        }


        public AwsF1Driver(ITimingReportParser timingReportParser) => _timingReportParser = timingReportParser;
    }
}
