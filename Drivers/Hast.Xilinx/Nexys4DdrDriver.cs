﻿using System;
using Hast.Synthesis;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Xilinx
{
    public class Nexys4DdrDriver : IDeviceDriver
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
+	unsigned128	unsigned128	sync	synth	5,643	-0,104
+	signed128	signed128	sync	synth	5,643	-0,104
+	unsigned32	unsigned32	sync	impl	3,156	0,024
+	signed32	signed32	sync	impl	3,156	0,024
+	unsigned64	unsigned64	sync	impl	3,898	-0,108
+	signed64	signed64	sync	impl	3,898	-0,108
+	unsigned16	unsigned16	sync	impl	2,939	-0,002
+	signed16	signed16	sync	impl	2,939	-0,002
+	unsigned8	unsigned8	sync	impl	2,276	-0,001
+	signed8	signed8	sync	impl	2,276	-0,001
>	unsigned128	boolean	sync	synth	3,902	-0,363
>	signed128	boolean	sync	synth	3,902	-0,363
>	unsigned32	boolean	sync	impl	2,755	-0,201
>	signed32	boolean	sync	impl	2,755	-0,201
>	unsigned64	boolean	sync	impl	2,999	-0,247
>	signed64	boolean	sync	impl	2,999	-0,247
>	unsigned16	boolean	sync	impl	2,212	-0,253
>	signed16	boolean	sync	impl	2,212	-0,253
>	unsigned8	boolean	sync	impl	2,058	-0,257
>	signed8	boolean	sync	impl	2,058	-0,257
<	unsigned128	boolean	sync	synth	3,902	-0,363
<	signed128	boolean	sync	synth	3,902	-0,363
<	unsigned32	boolean	sync	impl	2,610	-0,205
<	signed32	boolean	sync	impl	2,610	-0,205
<	unsigned64	boolean	sync	impl	2,897	-0,247
<	signed64	boolean	sync	impl	2,897	-0,247
<	unsigned16	boolean	sync	impl	2,212	-0,253
<	signed16	boolean	sync	impl	2,212	-0,253
<	unsigned8	boolean	sync	impl	2,099	-0,258
<	signed8	boolean	sync	impl	2,099	-0,258
>=	unsigned128	boolean	sync	synth	3,902	-0,363
>=	signed128	boolean	sync	synth	3,902	-0,363
>=	unsigned32	boolean	sync	impl	2,755	-0,201
>=	signed32	boolean	sync	impl	2,755	-0,201
>=	unsigned64	boolean	sync	impl	2,999	-0,247
>=	signed64	boolean	sync	impl	2,999	-0,247
>=	unsigned16	boolean	sync	impl	2,212	-0,253
>=	signed16	boolean	sync	impl	2,212	-0,253
>=	unsigned8	boolean	sync	impl	2,058	-0,257
>=	signed8	boolean	sync	impl	2,058	-0,257
<=	unsigned128	boolean	sync	synth	3,902	-0,363
<=	signed128	boolean	sync	synth	3,902	-0,363
<=	unsigned32	boolean	sync	impl	2,610	-0,205
<=	signed32	boolean	sync	impl	2,610	-0,205
<=	unsigned64	boolean	sync	impl	2,897	-0,247
<=	signed64	boolean	sync	impl	2,897	-0,247
<=	unsigned16	boolean	sync	impl	2,212	-0,253
<=	signed16	boolean	sync	impl	2,212	-0,253
<=	unsigned8	boolean	sync	impl	2,099	-0,258
<=	signed8	boolean	sync	impl	2,099	-0,258
=	unsigned128	boolean	sync	synth	3,576	-0,119
=	signed128	boolean	sync	synth	3,576	-0,119
=	unsigned32	boolean	sync	impl	2,751	-0,015
=	signed32	boolean	sync	impl	2,751	-0,015
=	unsigned64	boolean	sync	impl	2,891	-0,023
=	signed64	boolean	sync	impl	2,891	-0,023
=	unsigned16	boolean	sync	impl	2,440	0,037
=	signed16	boolean	sync	impl	2,440	0,037
=	unsigned8	boolean	sync	impl	2,260	-0,030
=	signed8	boolean	sync	impl	2,260	-0,030
/=	unsigned128	boolean	sync	synth	3,576	-0,119
/=	signed128	boolean	sync	synth	3,576	-0,119
/=	unsigned32	boolean	sync	impl	2,751	-0,015
/=	signed32	boolean	sync	impl	2,751	-0,015
/=	unsigned64	boolean	sync	impl	2,891	-0,023
/=	signed64	boolean	sync	impl	2,891	-0,023
/=	unsigned16	boolean	sync	impl	2,353	0,029
/=	signed16	boolean	sync	impl	2,353	0,029
/=	unsigned8	boolean	sync	impl	2,260	-0,030
/=	signed8	boolean	sync	impl	2,260	-0,030
-	unsigned128	unsigned128	sync	synth	5,643	-0,104
-	signed128	signed128	sync	synth	5,643	-0,104
-	unsigned32	unsigned32	sync	impl	3,156	0,024
-	signed32	signed32	sync	impl	3,156	0,024
-	unsigned64	unsigned64	sync	impl	3,898	-0,108
-	signed64	signed64	sync	impl	3,898	-0,108
-	unsigned16	unsigned16	sync	impl	2,939	-0,002
-	signed16	signed16	sync	impl	2,939	-0,002
-	unsigned8	unsigned8	sync	impl	2,276	-0,001
-	signed8	signed8	sync	impl	2,276	-0,001
/	unsigned128	unsigned128	sync	synth	677,589	-0,119
/	signed128	signed128	sync	synth	692,922	-0,136
/	unsigned32	unsigned32	sync	impl	85,347	-0,114
/	signed32	signed32	sync	impl	97,941	0,027
/	unsigned64	unsigned64	sync	impl	256,698	0,042
/	signed64	signed64	sync	impl	269,541	-0,142
/	unsigned16	unsigned16	sync	impl	32,060	-0,071
/	signed16	signed16	sync	impl	40,748	-0,042
/	unsigned8	unsigned8	sync	impl	13,953	-0,022
/	signed8	signed8	sync	impl	16,962	-0,037
*	unsigned128	unsigned256	sync	synth	7,632	-1,580
*	signed128	signed256	sync	synth	9,585	-0,104
*	unsigned32	unsigned64	sync	impl	5,558	-0,337
*	signed32	signed64	sync	impl	5,426	-0,337
*	unsigned64	unsigned128	sync	synth	7,632	-1,580
*	signed64	signed128	sync	synth	7,632	-1,580
*	unsigned16	unsigned32	sync	impl	7,488	0
*	signed16	signed32	sync	impl	7,728	0
*	unsigned8	unsigned16	sync	impl	6,827	-0,001
*	signed8	signed16	sync	impl	6,361	0,044
mod	unsigned128	unsigned128	sync	synth	684,293	-0,136
mod	signed128	signed128	sync	synth	711,833	-0,737
mod	unsigned32	unsigned32	sync	impl	90,102	-0,310
mod	signed32	signed32	sync	impl	99,441	-0,208
mod	unsigned64	unsigned64	sync	impl	259,775	-0,046
mod	signed64	signed64	sync	impl	273,363	-0,672
mod	unsigned16	unsigned16	sync	impl	35,100	-0,295
mod	signed16	signed16	sync	impl	40,313	-0,296
mod	unsigned8	unsigned8	sync	impl	15,090	-0,037
mod	signed8	signed8	sync	impl	19,436	-0,056
";
                    _timingReport = _timingReportParser.Parse(timingReport);
                }

                return _timingReport;
            }
        }

        public IDeviceManifest DeviceManifest { get; private set; }


        public Nexys4DdrDriver(ITimingReportParser timingReportParser)
        {
            _timingReportParser = timingReportParser;

            DeviceManifest = new DeviceManifest
            {
                TechnicalName = "Nexys4 DDR",
                ClockFrequencyHz = 100000000, // 100 Mhz
                SupportedCommunicationChannelNames = new[] { "Serial", "Ethernet" },
                AvailableMemoryBytes = 115343360 // 110MB
            };
        }


        public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, ushort operandSizeBits, bool isSigned)
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
                int intValue;

                if (int.TryParse(literalValue, out intValue))
                {
                    var log = Math.Log(intValue, 2);
                    // If the logarithm is a whole number that means that the value can be expressed as a power of 2.
                    if (log == Math.Floor(log))
                    {
                        return 0.1M;
                    }
                }
            }

            // With a 100 Mhz clock one clock cycle takes 10ns, so just need to divide by 10. 
            var latency = TimingReport.GetLatencyNs(binaryOperator, operandSizeBits, isSigned) / 10;

            // If there is no latency then let's try with a basic default (unless the operation is "instant" there should
            // be latency data).
            if (latency < 0) return 0.1M;

            return latency;
        }
    }
}
