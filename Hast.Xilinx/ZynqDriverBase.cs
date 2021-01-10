﻿using Hast.Synthesis;
using Hast.Synthesis.Helpers;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions.ManifestProviders;
using ICSharpCode.Decompiler.CSharp.Syntax;

namespace Hast.Xilinx
{
    public abstract class ZynqDriverBase : ZynqManifestProviderBase, IDeviceDriver
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
                    _timingReport ??= _timingReportParser.Parse(ResourceHelper.GetTimingReport(nameof(NexysDriverBase)));

                    return _timingReport;
                }
            }
        }

        public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForBinaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);

        public decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForUnaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);

        protected ZynqDriverBase(ITimingReportParser timingReportParser) => _timingReportParser = timingReportParser;
    }
}
