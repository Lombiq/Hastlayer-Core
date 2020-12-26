using Hast.Synthesis;
using Hast.Synthesis.Helpers;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions.ManifestProviders;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Xilinx
{
    public abstract class NexysDriverBase : NexysManifestProviderBase, IDeviceDriver
    {
        private readonly ITimingReportParser _timingReportParser;

        private readonly object _timingReportParserLock = new object();

        private ITimingReport _timingReport;
        private ITimingReport TimingReport
        {
            get
            {
                lock (_timingReportParserLock)
                {
                    _timingReport ??= _timingReportParser.Parse(ResourceHelper.GetTimingReport(nameof(NexysDriverBase)));

                    return _timingReport;
                }
            }
        }


        protected NexysDriverBase(ITimingReportParser timingReportParser)
        {
            _timingReportParser = timingReportParser;
        }


        public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForBinaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);

        public decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForUnaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);
    }
}
