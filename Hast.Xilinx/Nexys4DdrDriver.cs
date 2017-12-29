using System;
using Hast.Synthesis;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using Hast.Xilinx.Abstractions;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Xilinx
{
    public class Nexys4DdrDriver : Nexys4DdrManifestProvider, IDeviceDriver
    {
        private readonly ITimingReportParser _timingReportParser;

        private ITimingReport _timingReport;
        private ITimingReport TimingReport
        {
            get
            {
                if (_timingReport == null)
                {
                    var timingReport =
@"Op	InType	OutType	Template	DesignStat	DPD	TWD
shift_left	std_logic_vector1	std_logic_vector1	sync	synth	0	0
shift_left	std_logic_vector8	std_logic_vector8	sync	impl	1,070	-0,139
shift_left	std_logic_vector16	std_logic_vector16	sync	impl	1,221	-0,139
shift_left	std_logic_vector32	std_logic_vector32	sync	impl	1,221	-0,138
shift_left	std_logic_vector64	std_logic_vector64	sync	impl	1,047	-0,128
shift_left	std_logic_vector128	std_logic_vector128	sync	synth	0,812	-0,401
shift_right	std_logic_vector1	std_logic_vector1	sync	synth	0	0
shift_right	std_logic_vector8	std_logic_vector8	sync	impl	1,238	-0,139
shift_right	std_logic_vector16	std_logic_vector16	sync	impl	1,221	-0,139
shift_right	std_logic_vector32	std_logic_vector32	sync	impl	1,238	-0,138
shift_right	std_logic_vector64	std_logic_vector64	sync	impl	1,037	-0,128
shift_right	std_logic_vector128	std_logic_vector128	sync	synth	0,812	-0,401
not	std_logic_vector1	std_logic_vector1	sync	impl	1,002	-0,032
not	std_logic_vector8	std_logic_vector8	sync	impl	1,374	-0,035
not	std_logic_vector16	std_logic_vector16	sync	impl	1,771	-0,045
not	std_logic_vector32	std_logic_vector32	sync	impl	1,448	-0,032
not	std_logic_vector64	std_logic_vector64	sync	impl	1,441	-0,030
not	std_logic_vector128	std_logic_vector128	sync	synth	1,084	-0,137
and	std_logic_vector1	std_logic_vector1	sync	impl	1,238	-0,007
and	std_logic_vector8	std_logic_vector8	sync	impl	1,397	-0,007
and	std_logic_vector16	std_logic_vector16	sync	impl	1,390	-0,005
and	std_logic_vector32	std_logic_vector32	sync	impl	1,386	-0,031
and	std_logic_vector64	std_logic_vector64	sync	impl	1,734	-0,045
and	std_logic_vector128	std_logic_vector128	sync	synth	1,243	-0,137
nand	std_logic_vector1	std_logic_vector1	sync	impl	1,238	-0,007
nand	std_logic_vector8	std_logic_vector8	sync	impl	1,397	-0,007
nand	std_logic_vector16	std_logic_vector16	sync	impl	1,390	-0,005
nand	std_logic_vector32	std_logic_vector32	sync	impl	1,386	-0,031
nand	std_logic_vector64	std_logic_vector64	sync	impl	1,734	-0,045
nand	std_logic_vector128	std_logic_vector128	sync	synth	1,243	-0,137
or	std_logic_vector1	std_logic_vector1	sync	impl	1,238	-0,007
or	std_logic_vector8	std_logic_vector8	sync	impl	1,397	-0,007
or	std_logic_vector16	std_logic_vector16	sync	impl	1,390	-0,005
or	std_logic_vector32	std_logic_vector32	sync	impl	1,386	-0,031
or	std_logic_vector64	std_logic_vector64	sync	impl	1,734	-0,045
or	std_logic_vector128	std_logic_vector128	sync	synth	1,243	-0,137
nor	std_logic_vector1	std_logic_vector1	sync	impl	1,238	-0,007
nor	std_logic_vector8	std_logic_vector8	sync	impl	1,397	-0,007
nor	std_logic_vector16	std_logic_vector16	sync	impl	1,390	-0,005
nor	std_logic_vector32	std_logic_vector32	sync	impl	1,386	-0,031
nor	std_logic_vector64	std_logic_vector64	sync	impl	1,734	-0,045
nor	std_logic_vector128	std_logic_vector128	sync	synth	1,243	-0,137
xor	std_logic_vector1	std_logic_vector1	sync	impl	1,238	-0,007
xor	std_logic_vector8	std_logic_vector8	sync	impl	1,397	-0,007
xor	std_logic_vector16	std_logic_vector16	sync	impl	1,390	-0,005
xor	std_logic_vector32	std_logic_vector32	sync	impl	1,386	-0,031
xor	std_logic_vector64	std_logic_vector64	sync	impl	1,734	-0,045
xor	std_logic_vector128	std_logic_vector128	sync	synth	1,243	-0,137
xnor	std_logic_vector1	std_logic_vector1	sync	impl	1,238	-0,007
xnor	std_logic_vector8	std_logic_vector8	sync	impl	1,397	-0,007
xnor	std_logic_vector16	std_logic_vector16	sync	impl	1,390	-0,005
xnor	std_logic_vector32	std_logic_vector32	sync	impl	1,386	-0,031
xnor	std_logic_vector64	std_logic_vector64	sync	impl	1,734	-0,045
xnor	std_logic_vector128	std_logic_vector128	sync	synth	1,243	-0,137
add	unsigned1	unsigned1	sync	impl	1,238	-0,007
add	signed1	signed1	sync	impl	1,238	-0,007
add	unsigned8	unsigned8	sync	impl	2,433	0,000
add	signed8	signed8	sync	impl	2,433	0,000
add	unsigned16	unsigned16	sync	impl	2,661	-0,002
add	signed16	signed16	sync	impl	2,661	-0,002
add	unsigned32	unsigned32	sync	impl	3,263	-0,023
add	signed32	signed32	sync	impl	3,263	-0,023
add	unsigned64	unsigned64	sync	impl	3,898	-0,108
add	signed64	signed64	sync	impl	3,898	-0,108
add	unsigned128	unsigned128	sync	synth	5,643	-0,105
add	signed128	signed128	sync	synth	5,643	-0,105
gt	unsigned1	boolean	sync	impl	1,238	-0,007
gt	signed1	boolean	sync	impl	1,238	-0,007
gt	unsigned8	boolean	sync	impl	2,085	-0,260
gt	signed8	boolean	sync	impl	2,085	-0,260
gt	unsigned16	boolean	sync	impl	2,237	-0,253
gt	signed16	boolean	sync	impl	2,237	-0,253
gt	unsigned32	boolean	sync	impl	2,753	-0,249
gt	signed32	boolean	sync	impl	2,753	-0,249
gt	unsigned64	boolean	sync	impl	2,999	-0,247
gt	signed64	boolean	sync	impl	2,999	-0,247
gt	unsigned128	boolean	sync	synth	3,902	-0,364
gt	signed128	boolean	sync	synth	3,902	-0,364
lt	unsigned1	boolean	sync	impl	1,238	-0,007
lt	signed1	boolean	sync	impl	1,238	-0,007
lt	unsigned8	boolean	sync	synth	2,147	-0,364
lt	signed8	boolean	sync	synth	2,147	-0,364
lt	unsigned16	boolean	sync	impl	2,237	-0,253
lt	signed16	boolean	sync	impl	2,237	-0,253
lt	unsigned32	boolean	sync	impl	2,753	-0,249
lt	signed32	boolean	sync	impl	2,753	-0,249
lt	unsigned64	boolean	sync	impl	2,897	-0,247
lt	signed64	boolean	sync	impl	2,897	-0,247
lt	unsigned128	boolean	sync	synth	3,902	-0,364
lt	signed128	boolean	sync	synth	3,902	-0,364
ge	unsigned1	boolean	sync	impl	1,238	-0,007
ge	signed1	boolean	sync	impl	1,238	-0,007
ge	unsigned8	boolean	sync	impl	2,085	-0,260
ge	signed8	boolean	sync	impl	2,085	-0,260
ge	unsigned16	boolean	sync	impl	2,237	-0,253
ge	signed16	boolean	sync	impl	2,237	-0,253
ge	unsigned32	boolean	sync	impl	2,753	-0,249
ge	signed32	boolean	sync	impl	2,753	-0,249
ge	unsigned64	boolean	sync	impl	2,999	-0,247
ge	signed64	boolean	sync	impl	2,999	-0,247
ge	unsigned128	boolean	sync	synth	3,902	-0,364
ge	signed128	boolean	sync	synth	3,902	-0,364
le	unsigned1	boolean	sync	impl	1,238	-0,007
le	signed1	boolean	sync	impl	1,238	-0,007
le	unsigned8	boolean	sync	synth	2,147	-0,364
le	signed8	boolean	sync	synth	2,147	-0,364
le	unsigned16	boolean	sync	impl	2,237	-0,253
le	signed16	boolean	sync	impl	2,237	-0,253
le	unsigned32	boolean	sync	impl	2,753	-0,249
le	signed32	boolean	sync	impl	2,753	-0,249
le	unsigned64	boolean	sync	impl	2,897	-0,247
le	signed64	boolean	sync	impl	2,897	-0,247
le	unsigned128	boolean	sync	synth	3,902	-0,364
le	signed128	boolean	sync	synth	3,902	-0,364
eq	unsigned1	boolean	sync	impl	1,238	-0,007
eq	signed1	boolean	sync	impl	1,238	-0,007
eq	unsigned8	boolean	sync	impl	2,260	-0,030
eq	signed8	boolean	sync	impl	2,260	-0,030
eq	unsigned16	boolean	sync	impl	2,534	0,037
eq	signed16	boolean	sync	impl	2,534	0,037
eq	unsigned32	boolean	sync	synth	2,640	-0,120
eq	signed32	boolean	sync	synth	2,640	-0,120
eq	unsigned64	boolean	sync	impl	2,817	0,000
eq	signed64	boolean	sync	impl	2,817	0,000
eq	unsigned128	boolean	sync	synth	3,576	-0,120
eq	signed128	boolean	sync	synth	3,576	-0,120
neq	unsigned1	boolean	sync	impl	1,238	-0,007
neq	signed1	boolean	sync	impl	1,238	-0,007
neq	unsigned8	boolean	sync	impl	2,260	-0,030
neq	signed8	boolean	sync	impl	2,260	-0,030
neq	unsigned16	boolean	sync	impl	2,534	0,037
neq	signed16	boolean	sync	impl	2,534	0,037
neq	unsigned32	boolean	sync	synth	2,640	-0,120
neq	signed32	boolean	sync	synth	2,640	-0,120
neq	unsigned64	boolean	sync	impl	2,817	0,000
neq	signed64	boolean	sync	impl	2,817	0,000
neq	unsigned128	boolean	sync	synth	3,576	-0,120
neq	signed128	boolean	sync	synth	3,576	-0,120
sub	unsigned1	unsigned1	sync	impl	1,238	-0,007
sub	signed1	signed1	sync	impl	1,238	-0,007
sub	unsigned8	unsigned8	sync	impl	2,433	0,000
sub	signed8	signed8	sync	impl	2,433	0,000
sub	unsigned16	unsigned16	sync	impl	2,661	-0,002
sub	signed16	signed16	sync	impl	2,661	-0,002
sub	unsigned32	unsigned32	sync	impl	3,263	-0,023
sub	signed32	signed32	sync	impl	3,263	-0,023
sub	unsigned64	unsigned64	sync	impl	3,898	-0,108
sub	signed64	signed64	sync	impl	3,898	-0,108
sub	unsigned128	unsigned128	sync	synth	5,643	-0,105
sub	signed128	signed128	sync	synth	5,643	-0,105
div	unsigned1	unsigned1	sync	impl	1,238	-0,007
div	signed1	signed1	sync	impl	1,238	-0,007
div	unsigned8	unsigned8	sync	synth	14,133	-0,120
div	signed8	signed8	sync	synth	17,213	-0,137
div	unsigned16	unsigned16	sync	impl	32,345	-0,121
div	signed16	signed16	sync	impl	39,414	-0,045
div	unsigned32	unsigned32	sync	impl	85,815	-0,066
div	signed32	signed32	sync	impl	97,992	-0,123
div	unsigned64	unsigned64	sync	impl	255,265	-0,153
div	signed64	signed64	sync	impl	268,048	-0,142
div	unsigned128	unsigned128	sync	synth	677,502	-0,120
div	signed128	signed128	sync	synth	692,755	-0,137
mul	unsigned1	unsigned2	sync	impl	1,238	-0,007
mul	signed1	signed2	sync	impl	1,238	-0,007
mul	unsigned8	unsigned16	sync	impl	6,472	-0,001
mul	signed8	signed16	sync	impl	6,376	-0,003
mul	unsigned16	unsigned32	sync	impl	7,488	0
mul	signed16	signed32	sync	impl	7,728	0
mul	unsigned32	unsigned64	sync	impl	5,358	-0,334
mul	signed32	signed64	sync	impl	5,436	-0,331
mul	unsigned64	unsigned128	sync	synth	7,632	-1,581
mul	signed64	signed128	sync	synth	7,632	-1,581
mul	unsigned128	unsigned256	sync	synth	7,632	-1,581
mul	signed128	signed256	sync	synth	9,585	-0,105
mod	unsigned1	unsigned1	sync	impl	1,238	-0,007
mod	signed1	signed1	sync	impl	1,238	-0,007
mod	unsigned8	unsigned8	sync	synth	16,154	-0,137
mod	signed8	signed8	sync	synth	20,475	-0,137
mod	unsigned16	unsigned16	sync	synth	34,182	-0,137
mod	signed16	signed16	sync	impl	40,125	-0,299
mod	unsigned32	unsigned32	sync	impl	89,215	-0,272
mod	signed32	signed32	sync	impl	99,372	-0,204
mod	unsigned64	unsigned64	sync	impl	259,328	-0,158
mod	signed64	signed64	sync	impl	273,871	-0,505
mod	unsigned128	unsigned128	sync	synth	684,217	-0,137
mod	signed128	signed128	sync	synth	711,757	-0,738
";
                    _timingReport = _timingReportParser.Parse(timingReport);
                }

                return _timingReport;
            }
        }


        public Nexys4DdrDriver(ITimingReportParser timingReportParser)
        {
            _timingReportParser = timingReportParser;
        }


        public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned)
        {
            var binaryOperator = expression.Operator;

            // If the Right expression results in 2^n then since the operations will be implemented with a very compact 
            // circuit (just with wiring) we can assume that it's "instant".
            if ((binaryOperator == BinaryOperatorType.Multiply || binaryOperator == BinaryOperatorType.Divide) &&
                expression.Right is PrimitiveExpression)
            {
                // LiteralValue somehow is an empty string for PrimitiveExpressions.
                var valueObject = ((PrimitiveExpression)expression.Right).Value;
                var literalValue = valueObject != null ? valueObject.ToString() : string.Empty;

                if (int.TryParse(literalValue, out var intValue))
                {
                    var log = Math.Log(intValue, 2);
                    // If the logarithm is a whole number that means that the value can be expressed as a power of 2.
                    if (log == Math.Floor(log))
                    {
                        return 0.1M;
                    }
                }
            }

            return ComputeClockCyclesFromLatency(TimingReport.GetLatencyNs(binaryOperator, operandSizeBits, isSigned));
        }

        public decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            ComputeClockCyclesFromLatency(TimingReport.GetLatencyNs(expression.Operator, operandSizeBits, isSigned));


        private decimal ComputeClockCyclesFromLatency(decimal latencyNs)
        {
            var latencyClockCycles = latencyNs * (DeviceManifest.ClockFrequencyHz * 0.000000001M);

            // If there is no latency then let's try with a basic default (unless the operation is "instant" there should
            // be latency data).
            if (latencyClockCycles < 0) return 0.1M;

            return latencyClockCycles;
        }
    }
}
