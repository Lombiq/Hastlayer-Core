using System;
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
+	unsigned128	unsigned128	sync	synth	5,643	-0,1040001
+	signed128	signed128	sync	synth	5,643	-0,1040001
+	unsigned32	unsigned32	sync	impl	3,156	0,02400017
+	signed32	signed32	sync	impl	3,156	0,02400017
+	unsigned64	unsigned64	sync	impl	3,898	-0,1079998
+	signed64	signed64	sync	impl	3,898	-0,1079998
+	unsigned16	unsigned16	sync	impl	2,939	-0,001999855
+	signed16	signed16	sync	impl	2,939	-0,001999855
+	unsigned8	unsigned8	sync	impl	2,276	-0,001000404
+	signed8	signed8	sync	impl	2,276	-0,001000404
>	unsigned128	boolean	sync	impl	2,276	-0,001000404
>	signed128	boolean	sync	impl	2,276	-0,001000404
>	unsigned32	boolean	sync	impl	2,755	-0,2010002
>	signed32	boolean	sync	impl	2,755	-0,2010002
>	unsigned64	boolean	sync	impl	2,999	-0,2469997
>	signed64	boolean	sync	impl	2,999	-0,2469997
>	unsigned16	boolean	sync	impl	2,212	-0,2530003
>	signed16	boolean	sync	impl	2,212	-0,2530003
>	unsigned8	boolean	sync	impl	2,058	-0,257
>	signed8	boolean	sync	impl	2,058	-0,257
<	unsigned128	boolean	sync	impl	2,058	-0,257
<	signed128	boolean	sync	impl	2,058	-0,257
<	unsigned32	boolean	sync	impl	2,61	-0,2049999
<	signed32	boolean	sync	impl	2,61	-0,2049999
<	unsigned64	boolean	sync	impl	2,897	-0,2469997
<	signed64	boolean	sync	impl	2,897	-0,2469997
<	unsigned16	boolean	sync	impl	2,212	-0,2530003
<	signed16	boolean	sync	impl	2,212	-0,2530003
<	unsigned8	boolean	sync	impl	2,099	-0,2580004
<	signed8	boolean	sync	impl	2,099	-0,2580004
>=	unsigned128	boolean	sync	impl	2,099	-0,2580004
>=	signed128	boolean	sync	impl	2,099	-0,2580004
>=	unsigned32	boolean	sync	impl	2,755	-0,2010002
>=	signed32	boolean	sync	impl	2,755	-0,2010002
>=	unsigned64	boolean	sync	impl	2,999	-0,2469997
>=	signed64	boolean	sync	impl	2,999	-0,2469997
>=	unsigned16	boolean	sync	impl	2,212	-0,2530003
>=	signed16	boolean	sync	impl	2,212	-0,2530003
>=	unsigned8	boolean	sync	impl	2,058	-0,257
>=	signed8	boolean	sync	impl	2,058	-0,257
<=	unsigned128	boolean	sync	impl	2,058	-0,257
<=	signed128	boolean	sync	impl	2,058	-0,257
<=	unsigned32	boolean	sync	impl	2,61	-0,2049999
<=	signed32	boolean	sync	impl	2,61	-0,2049999
<=	unsigned64	boolean	sync	impl	2,897	-0,2469997
<=	signed64	boolean	sync	impl	2,897	-0,2469997
<=	unsigned16	boolean	sync	impl	2,212	-0,2530003
<=	signed16	boolean	sync	impl	2,212	-0,2530003
<=	unsigned8	boolean	sync	impl	2,099	-0,2580004
<=	signed8	boolean	sync	impl	2,099	-0,2580004
=	unsigned128	boolean	sync	impl	2,099	-0,2580004
=	signed128	boolean	sync	impl	2,099	-0,2580004
=	unsigned32	boolean	sync	impl	2,751	-0,01499939
=	signed32	boolean	sync	impl	2,751	-0,01499939
=	unsigned64	boolean	sync	impl	2,891	-0,02299976
=	signed64	boolean	sync	impl	2,891	-0,02299976
=	unsigned16	boolean	sync	impl	2,44	0,0369997
=	signed16	boolean	sync	impl	2,44	0,0369997
=	unsigned8	boolean	sync	impl	2,26	-0,03000069
=	signed8	boolean	sync	impl	2,26	-0,03000069
/=	unsigned128	boolean	sync	impl	2,26	-0,03000069
/=	signed128	boolean	sync	impl	2,26	-0,03000069
/=	unsigned32	boolean	sync	impl	2,751	-0,01499939
/=	signed32	boolean	sync	impl	2,751	-0,01499939
/=	unsigned64	boolean	sync	impl	2,891	-0,02299976
/=	signed64	boolean	sync	impl	2,891	-0,02299976
/=	unsigned16	boolean	sync	impl	2,353	0,02899933
/=	signed16	boolean	sync	impl	2,353	0,02899933
/=	unsigned8	boolean	sync	impl	2,26	-0,03000069
/=	signed8	boolean	sync	impl	2,26	-0,03000069
-	unsigned128	unsigned128	sync	impl	2,26	-0,03000069
-	signed128	signed128	sync	impl	2,26	-0,03000069
-	unsigned32	unsigned32	sync	impl	3,156	0,02400017
-	signed32	signed32	sync	impl	3,156	0,02400017
-	unsigned64	unsigned64	sync	impl	3,898	-0,1079998
-	signed64	signed64	sync	impl	3,898	-0,1079998
-	unsigned16	unsigned16	sync	impl	2,939	-0,001999855
-	signed16	signed16	sync	impl	2,939	-0,001999855
-	unsigned8	unsigned8	sync	impl	2,276	-0,001000404
-	signed8	signed8	sync	impl	2,276	-0,001000404
/	unsigned128	unsigned128	sync	impl	2,276	-0,001000404
/	signed128	signed128	sync	impl	2,276	-0,001000404
/	unsigned32	unsigned32	sync	impl	85,347	-0,1140003
/	signed32	signed32	sync	impl	97,941	0,02700043
/	unsigned64	unsigned64	sync	impl	256,698	0,04199982
/	signed64	signed64	sync	impl	269,541	-0,1420002
/	unsigned16	unsigned16	sync	impl	32,06	-0,0710001
/	signed16	signed16	sync	impl	40,748	-0,04199982
/	unsigned8	unsigned8	sync	impl	13,953	-0,02199936
/	signed8	signed8	sync	impl	16,962	-0,0369997
*	unsigned128	unsigned256	sync	impl	16,962	-0,0369997
*	signed128	signed256	sync	impl	16,962	-0,0369997
*	unsigned32	unsigned64	sync	impl	5,558	-0,3369999
*	signed32	signed64	sync	impl	5,426	-0,3369999
*	unsigned64	unsigned128	sync	impl	5,426	-0,3369999
*	signed64	signed128	sync	impl	5,426	-0,3369999
*	unsigned16	unsigned32	sync	impl	7,488	0
*	signed16	signed32	sync	impl	7,728	0
*	unsigned8	unsigned16	sync	impl	6,827	-0,0009994507
*	signed8	signed16	sync	impl	6,361	0,04399967
mod	unsigned128	unsigned128	sync	impl	6,361	0,04399967
mod	signed128	signed128	sync	impl	6,361	0,04399967
mod	unsigned32	unsigned32	sync	impl	90,102	-0,3100004
mod	signed32	signed32	sync	impl	99,441	-0,2080002
mod	unsigned64	unsigned64	sync	impl	259,775	-0,04599953
mod	signed64	signed64	sync	impl	273,363	-0,6720009
mod	unsigned16	unsigned16	sync	impl	35,1	-0,2950001
mod	signed16	signed16	sync	impl	40,313	-0,2959995
mod	unsigned8	unsigned8	sync	impl	15,09	-0,0369997
mod	signed8	signed8	sync	impl	19,436	-0,05599976
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
            return TimingReport.GetLatencyNs(binaryOperator, operandSizeBits, isSigned) / 10;
        }
    }
}
