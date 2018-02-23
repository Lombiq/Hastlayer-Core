using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Catapult.Abstractions;
using Hast.Synthesis;
using Hast.Synthesis.Helpers;
using Hast.Synthesis.Models;
using Hast.Synthesis.Services;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Catapult
{
    public class CatapultDriver : CatapultManifestProvider, IDeviceDriver
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
                    if (_timingReport == null)
                    {
                        var timingReport =
@"Op	InType	OutType	Template	DesignStat	DPD	TWDFR
not	std_logic_vector32	std_logic_vector32	sync	impl	0,801	0,136
not	std_logic_vector64	std_logic_vector64	sync	impl	1,347	0,108
and	std_logic_vector32	std_logic_vector32	sync	impl	0,864	0,133
and	std_logic_vector64	std_logic_vector64	sync	impl	1,486	0,007
nand	std_logic_vector32	std_logic_vector32	sync	impl	0,940	0,126
nand	std_logic_vector64	std_logic_vector64	sync	impl	0,944	0,017
or	std_logic_vector32	std_logic_vector32	sync	impl	0,864	0,133
or	std_logic_vector64	std_logic_vector64	sync	impl	1,486	0,007
nor	std_logic_vector32	std_logic_vector32	sync	impl	0,940	0,126
nor	std_logic_vector64	std_logic_vector64	sync	impl	0,944	0,017
xor	std_logic_vector32	std_logic_vector32	sync	impl	0,942	0,126
xor	std_logic_vector64	std_logic_vector64	sync	impl	1,486	0,007
xnor	std_logic_vector32	std_logic_vector32	sync	impl	0,942	0,126
xnor	std_logic_vector64	std_logic_vector64	sync	impl	1,486	0,007
add	unsigned32	unsigned32	sync	impl	2,654	0,027
add	signed32	signed32	sync	impl	2,654	0,027
add	unsigned64	unsigned64	sync	impl	3,333	0,062
add	signed64	signed64	sync	impl	3,333	0,062
gt	unsigned32	boolean	sync	impl	3,369	0,124
gt	signed32	boolean	sync	impl	3,369	0,124
gt	unsigned64	boolean	sync	impl	4,220	0,092
gt	signed64	boolean	sync	impl	4,220	0,092
lt	unsigned32	boolean	sync	impl	3,204	0,113
lt	signed32	boolean	sync	impl	3,101	0,113
lt	unsigned64	boolean	sync	impl	4,087	0,153
lt	signed64	boolean	sync	impl	4,087	0,153
ge	unsigned32	boolean	sync	impl	3,212	0,105
ge	signed32	boolean	sync	impl	3,212	0,105
ge	unsigned64	boolean	sync	impl	4,792	0,122
ge	signed64	boolean	sync	impl	4,792	0,122
le	unsigned32	boolean	sync	impl	3,469	0,015
le	signed32	boolean	sync	impl	3,469	0,015
le	unsigned64	boolean	sync	impl	4,775	0,104
le	signed64	boolean	sync	impl	4,775	0,104
eq	unsigned32	boolean	sync	impl	2,252	0,122
eq	signed32	boolean	sync	impl	2,252	0,122
eq	unsigned64	boolean	sync	impl	2,914	0,027
eq	signed64	boolean	sync	impl	2,914	0,027
neq	unsigned32	boolean	sync	impl	2,255	0,122
neq	signed32	boolean	sync	impl	2,255	0,122
neq	unsigned64	boolean	sync	impl	2,908	0,027
neq	signed64	boolean	sync	impl	2,908	0,027
sub	unsigned32	unsigned32	sync	impl	2,376	0,129
sub	signed32	signed32	sync	impl	2,376	0,129
sub	unsigned64	unsigned64	sync	impl	2,657	0,127
sub	signed64	signed64	sync	impl	2,657	0,127
div	unsigned32	unsigned32	sync	impl	50,269	0,094
div	signed32	signed32	sync	impl	52,288	0,068
div	unsigned64	unsigned64	sync	impl	129,530	0,078
div	signed64	signed64	sync	impl	132,317	0,034
mul	unsigned32	unsigned64	sync	impl	2,548	-0,154
mul	signed32	signed64	sync	impl	2,548	-0,156
mul	unsigned64	unsigned128	sync	impl	7,445	-0,147
mul	signed64	signed128	sync	impl	7,161	-0,162
mod	unsigned32	unsigned32	sync	impl	51,412	0,083
mod	signed32	signed32	sync	impl	54,490	0,026
mod	unsigned64	unsigned64	sync	impl	132,014	0,077
mod	signed64	signed64	sync	impl	136,143	0,062
dotnet_shift_left_by_0	unsigned64	unsigned64	sync	impl	1,085	0,120
dotnet_shift_left_by_0	signed64	signed64	sync	impl	1,085	0,120
dotnet_shift_right_by_0	unsigned64	unsigned64	sync	impl	1,085	0,120
dotnet_shift_right_by_0	signed64	signed64	sync	impl	1,085	0,120
dotnet_shift_left_by_0	unsigned32	unsigned32	sync	impl	0,793	0,112
dotnet_shift_left_by_0	signed32	signed32	sync	impl	0,793	0,112
dotnet_shift_right_by_0	unsigned32	unsigned32	sync	impl	0,793	0,112
dotnet_shift_right_by_0	signed32	signed32	sync	impl	0,793	0,112
dotnet_shift_left_by_1	unsigned64	unsigned64	sync	impl	0,988	0,125
dotnet_shift_left_by_1	signed64	signed64	sync	impl	0,988	0,125
dotnet_shift_right_by_1	unsigned64	unsigned64	sync	impl	1,626	0,059
dotnet_shift_right_by_1	signed64	signed64	sync	impl	0,948	0,118
dotnet_shift_left_by_1	unsigned32	unsigned32	sync	impl	1,014	0,136
dotnet_shift_left_by_1	signed32	signed32	sync	impl	1,014	0,136
dotnet_shift_right_by_1	unsigned32	unsigned32	sync	impl	0,965	0,135
dotnet_shift_right_by_1	signed32	signed32	sync	impl	1,056	0,140
dotnet_shift_left_by_2	unsigned64	unsigned64	sync	impl	1,128	0,122
dotnet_shift_left_by_2	signed64	signed64	sync	impl	1,128	0,122
dotnet_shift_right_by_2	unsigned64	unsigned64	sync	impl	1,153	0,141
dotnet_shift_right_by_2	signed64	signed64	sync	impl	1,122	0,121
dotnet_shift_left_by_2	unsigned32	unsigned32	sync	impl	0,969	0,137
dotnet_shift_left_by_2	signed32	signed32	sync	impl	0,969	0,137
dotnet_shift_right_by_2	unsigned32	unsigned32	sync	impl	0,965	0,135
dotnet_shift_right_by_2	signed32	signed32	sync	impl	0,966	0,138
dotnet_shift_left_by_3	unsigned64	unsigned64	sync	impl	0,993	0,119
dotnet_shift_left_by_3	signed64	signed64	sync	impl	0,993	0,119
dotnet_shift_right_by_3	unsigned64	unsigned64	sync	impl	0,996	0,142
dotnet_shift_right_by_3	signed64	signed64	sync	impl	1,056	0,145
dotnet_shift_left_by_3	unsigned32	unsigned32	sync	impl	0,965	0,136
dotnet_shift_left_by_3	signed32	signed32	sync	impl	0,965	0,136
dotnet_shift_right_by_3	unsigned32	unsigned32	sync	impl	1,014	0,136
dotnet_shift_right_by_3	signed32	signed32	sync	impl	1,018	0,120
dotnet_shift_left_by_4	unsigned64	unsigned64	sync	impl	1,151	0,157
dotnet_shift_left_by_4	signed64	signed64	sync	impl	1,151	0,157
dotnet_shift_right_by_4	unsigned64	unsigned64	sync	impl	1,184	0,150
dotnet_shift_right_by_4	signed64	signed64	sync	impl	0,975	0,141
dotnet_shift_left_by_4	unsigned32	unsigned32	sync	impl	0,943	0,121
dotnet_shift_left_by_4	signed32	signed32	sync	impl	0,943	0,121
dotnet_shift_right_by_4	unsigned32	unsigned32	sync	impl	1,014	0,137
dotnet_shift_right_by_4	signed32	signed32	sync	impl	0,944	0,121
dotnet_shift_left_by_5	unsigned64	unsigned64	sync	impl	1,643	0,134
dotnet_shift_left_by_5	signed64	signed64	sync	impl	1,643	0,134
dotnet_shift_right_by_5	unsigned64	unsigned64	sync	impl	0,977	0,124
dotnet_shift_right_by_5	signed64	signed64	sync	impl	1,212	0,257
dotnet_shift_left_by_5	unsigned32	unsigned32	sync	impl	0,965	0,136
dotnet_shift_left_by_5	signed32	signed32	sync	impl	0,965	0,136
dotnet_shift_right_by_5	unsigned32	unsigned32	sync	impl	0,945	0,118
dotnet_shift_right_by_5	signed32	signed32	sync	impl	1,174	0,041
dotnet_shift_left_by_6	unsigned64	unsigned64	sync	impl	1,186	0,133
dotnet_shift_left_by_6	signed64	signed64	sync	impl	1,186	0,133
dotnet_shift_right_by_6	unsigned64	unsigned64	sync	impl	1,046	0,128
dotnet_shift_right_by_6	signed64	signed64	sync	impl	1,052	0,123
dotnet_shift_left_by_6	unsigned32	unsigned32	sync	impl	0,965	0,136
dotnet_shift_left_by_6	signed32	signed32	sync	impl	0,965	0,136
dotnet_shift_right_by_6	unsigned32	unsigned32	sync	impl	0,966	0,136
dotnet_shift_right_by_6	signed32	signed32	sync	impl	0,977	0,108
dotnet_shift_left_by_7	unsigned64	unsigned64	sync	impl	0,969	0,136
dotnet_shift_left_by_7	signed64	signed64	sync	impl	0,969	0,136
dotnet_shift_right_by_7	unsigned64	unsigned64	sync	impl	1,085	0,119
dotnet_shift_right_by_7	signed64	signed64	sync	impl	1,048	0,121
dotnet_shift_left_by_7	unsigned32	unsigned32	sync	impl	0,964	0,136
dotnet_shift_left_by_7	signed32	signed32	sync	impl	0,964	0,136
dotnet_shift_right_by_7	unsigned32	unsigned32	sync	impl	0,964	0,138
dotnet_shift_right_by_7	signed32	signed32	sync	impl	1,025	0,130
dotnet_shift_left_by_8	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_left_by_8	signed64	signed64	sync	impl	0,965	0,135
dotnet_shift_right_by_8	unsigned64	unsigned64	sync	impl	1,046	0,127
dotnet_shift_right_by_8	signed64	signed64	sync	impl	1,047	0,120
dotnet_shift_left_by_8	unsigned32	unsigned32	sync	impl	0,965	0,138
dotnet_shift_left_by_8	signed32	signed32	sync	impl	0,965	0,138
dotnet_shift_right_by_8	unsigned32	unsigned32	sync	impl	0,965	0,138
dotnet_shift_right_by_8	signed32	signed32	sync	impl	1,636	0,059
dotnet_shift_left_by_9	unsigned64	unsigned64	sync	impl	0,954	0,117
dotnet_shift_left_by_9	signed64	signed64	sync	impl	0,954	0,117
dotnet_shift_right_by_9	unsigned64	unsigned64	sync	impl	1,128	0,123
dotnet_shift_right_by_9	signed64	signed64	sync	impl	1,154	0,141
dotnet_shift_left_by_9	unsigned32	unsigned32	sync	impl	0,965	0,138
dotnet_shift_left_by_9	signed32	signed32	sync	impl	0,965	0,138
dotnet_shift_right_by_9	unsigned32	unsigned32	sync	impl	1,001	0,134
dotnet_shift_right_by_9	signed32	signed32	sync	impl	1,191	0,123
dotnet_shift_left_by_10	unsigned64	unsigned64	sync	impl	1,191	0,129
dotnet_shift_left_by_10	signed64	signed64	sync	impl	1,191	0,129
dotnet_shift_right_by_10	unsigned64	unsigned64	sync	impl	0,986	0,133
dotnet_shift_right_by_10	signed64	signed64	sync	impl	1,291	0,141
dotnet_shift_left_by_10	unsigned32	unsigned32	sync	impl	0,807	0,153
dotnet_shift_left_by_10	signed32	signed32	sync	impl	0,807	0,153
dotnet_shift_right_by_10	unsigned32	unsigned32	sync	impl	0,965	0,137
dotnet_shift_right_by_10	signed32	signed32	sync	impl	1,086	0,121
dotnet_shift_left_by_11	unsigned64	unsigned64	sync	impl	0,954	0,121
dotnet_shift_left_by_11	signed64	signed64	sync	impl	0,954	0,121
dotnet_shift_right_by_11	unsigned64	unsigned64	sync	impl	1,000	0,149
dotnet_shift_right_by_11	signed64	signed64	sync	impl	0,964	0,136
dotnet_shift_left_by_11	unsigned32	unsigned32	sync	impl	0,965	0,137
dotnet_shift_left_by_11	signed32	signed32	sync	impl	0,965	0,137
dotnet_shift_right_by_11	unsigned32	unsigned32	sync	impl	0,965	0,139
dotnet_shift_right_by_11	signed32	signed32	sync	impl	1,124	0,127
dotnet_shift_left_by_12	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_left_by_12	signed64	signed64	sync	impl	0,965	0,135
dotnet_shift_right_by_12	unsigned64	unsigned64	sync	impl	1,010	0,140
dotnet_shift_right_by_12	signed64	signed64	sync	impl	1,334	0,143
dotnet_shift_left_by_12	unsigned32	unsigned32	sync	impl	0,965	0,136
dotnet_shift_left_by_12	signed32	signed32	sync	impl	0,965	0,136
dotnet_shift_right_by_12	unsigned32	unsigned32	sync	impl	0,984	0,140
dotnet_shift_right_by_12	signed32	signed32	sync	impl	1,111	0,120
dotnet_shift_left_by_13	unsigned64	unsigned64	sync	impl	1,026	0,114
dotnet_shift_left_by_13	signed64	signed64	sync	impl	1,026	0,114
dotnet_shift_right_by_13	unsigned64	unsigned64	sync	impl	1,127	0,125
dotnet_shift_right_by_13	signed64	signed64	sync	impl	1,181	0,040
dotnet_shift_left_by_13	unsigned32	unsigned32	sync	impl	0,965	0,136
dotnet_shift_left_by_13	signed32	signed32	sync	impl	0,965	0,136
dotnet_shift_right_by_13	unsigned32	unsigned32	sync	impl	1,011	0,141
dotnet_shift_right_by_13	signed32	signed32	sync	impl	1,320	0,138
dotnet_shift_left_by_14	unsigned64	unsigned64	sync	impl	0,966	0,135
dotnet_shift_left_by_14	signed64	signed64	sync	impl	0,966	0,135
dotnet_shift_right_by_14	unsigned64	unsigned64	sync	impl	1,013	0,136
dotnet_shift_right_by_14	signed64	signed64	sync	impl	3,499	0,016
dotnet_shift_left_by_14	unsigned32	unsigned32	sync	impl	0,806	0,154
dotnet_shift_left_by_14	signed32	signed32	sync	impl	0,806	0,154
dotnet_shift_right_by_14	unsigned32	unsigned32	sync	impl	0,965	0,137
dotnet_shift_right_by_14	signed32	signed32	sync	impl	1,387	0,122
dotnet_shift_left_by_15	unsigned64	unsigned64	sync	impl	1,064	0,145
dotnet_shift_left_by_15	signed64	signed64	sync	impl	1,064	0,145
dotnet_shift_right_by_15	unsigned64	unsigned64	sync	impl	0,965	0,136
dotnet_shift_right_by_15	signed64	signed64	sync	impl	1,334	0,123
dotnet_shift_left_by_15	unsigned32	unsigned32	sync	impl	0,944	0,118
dotnet_shift_left_by_15	signed32	signed32	sync	impl	0,944	0,118
dotnet_shift_right_by_15	unsigned32	unsigned32	sync	impl	0,965	0,137
dotnet_shift_right_by_15	signed32	signed32	sync	impl	1,129	0,121
dotnet_shift_left_by_16	unsigned64	unsigned64	sync	impl	1,543	0,154
dotnet_shift_left_by_16	signed64	signed64	sync	impl	1,543	0,154
dotnet_shift_right_by_16	unsigned64	unsigned64	sync	impl	1,359	0,162
dotnet_shift_right_by_16	signed64	signed64	sync	impl	1,152	0,139
dotnet_shift_left_by_16	unsigned32	unsigned32	sync	impl	0,875	0,025
dotnet_shift_left_by_16	signed32	signed32	sync	impl	0,875	0,025
dotnet_shift_right_by_16	unsigned32	unsigned32	sync	impl	0,964	0,136
dotnet_shift_right_by_16	signed32	signed32	sync	impl	0,867	0,154
dotnet_shift_left_by_17	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_left_by_17	signed64	signed64	sync	impl	0,965	0,135
dotnet_shift_right_by_17	unsigned64	unsigned64	sync	impl	0,984	0,144
dotnet_shift_right_by_17	signed64	signed64	sync	impl	1,090	0,121
dotnet_shift_left_by_17	unsigned32	unsigned32	sync	impl	1,084	0,121
dotnet_shift_left_by_17	signed32	signed32	sync	impl	1,084	0,121
dotnet_shift_right_by_17	unsigned32	unsigned32	sync	impl	0,945	0,118
dotnet_shift_right_by_17	signed32	signed32	sync	impl	1,359	0,138
dotnet_shift_left_by_18	unsigned64	unsigned64	sync	impl	1,127	0,120
dotnet_shift_left_by_18	signed64	signed64	sync	impl	1,127	0,120
dotnet_shift_right_by_18	unsigned64	unsigned64	sync	impl	1,053	0,127
dotnet_shift_right_by_18	signed64	signed64	sync	impl	1,253	0,117
dotnet_shift_left_by_18	unsigned32	unsigned32	sync	impl	0,945	0,118
dotnet_shift_left_by_18	signed32	signed32	sync	impl	0,945	0,118
dotnet_shift_right_by_18	unsigned32	unsigned32	sync	impl	1,085	0,124
dotnet_shift_right_by_18	signed32	signed32	sync	impl	1,296	0,144
dotnet_shift_left_by_19	unsigned64	unsigned64	sync	impl	0,945	0,120
dotnet_shift_left_by_19	signed64	signed64	sync	impl	0,945	0,120
dotnet_shift_right_by_19	unsigned64	unsigned64	sync	impl	1,289	0,132
dotnet_shift_right_by_19	signed64	signed64	sync	impl	1,090	0,121
dotnet_shift_left_by_19	unsigned32	unsigned32	sync	impl	1,014	0,138
dotnet_shift_left_by_19	signed32	signed32	sync	impl	1,014	0,138
dotnet_shift_right_by_19	unsigned32	unsigned32	sync	impl	0,965	0,139
dotnet_shift_right_by_19	signed32	signed32	sync	impl	1,288	0,138
dotnet_shift_left_by_20	unsigned64	unsigned64	sync	impl	0,964	0,136
dotnet_shift_left_by_20	signed64	signed64	sync	impl	0,964	0,136
dotnet_shift_right_by_20	unsigned64	unsigned64	sync	impl	0,964	0,136
dotnet_shift_right_by_20	signed64	signed64	sync	impl	1,205	0,040
dotnet_shift_left_by_20	unsigned32	unsigned32	sync	impl	0,966	0,136
dotnet_shift_left_by_20	signed32	signed32	sync	impl	0,966	0,136
dotnet_shift_right_by_20	unsigned32	unsigned32	sync	impl	0,966	0,136
dotnet_shift_right_by_20	signed32	signed32	sync	impl	0,960	0,091
dotnet_shift_left_by_21	unsigned64	unsigned64	sync	impl	1,084	0,120
dotnet_shift_left_by_21	signed64	signed64	sync	impl	1,084	0,120
dotnet_shift_right_by_21	unsigned64	unsigned64	sync	impl	0,966	0,136
dotnet_shift_right_by_21	signed64	signed64	sync	impl	2,699	0,174
dotnet_shift_left_by_21	unsigned32	unsigned32	sync	impl	0,945	0,118
dotnet_shift_left_by_21	signed32	signed32	sync	impl	0,945	0,118
dotnet_shift_right_by_21	unsigned32	unsigned32	sync	impl	0,966	0,136
dotnet_shift_right_by_21	signed32	signed32	sync	impl	1,191	0,120
dotnet_shift_left_by_22	unsigned64	unsigned64	sync	impl	0,964	0,135
dotnet_shift_left_by_22	signed64	signed64	sync	impl	0,964	0,135
dotnet_shift_right_by_22	unsigned64	unsigned64	sync	impl	1,746	0,122
dotnet_shift_right_by_22	signed64	signed64	sync	impl	2,284	0,017
dotnet_shift_left_by_22	unsigned32	unsigned32	sync	impl	0,965	0,138
dotnet_shift_left_by_22	signed32	signed32	sync	impl	0,965	0,138
dotnet_shift_right_by_22	unsigned32	unsigned32	sync	impl	0,964	0,137
dotnet_shift_right_by_22	signed32	signed32	sync	impl	1,368	0,136
dotnet_shift_left_by_23	unsigned64	unsigned64	sync	impl	1,011	0,143
dotnet_shift_left_by_23	signed64	signed64	sync	impl	1,011	0,143
dotnet_shift_right_by_23	unsigned64	unsigned64	sync	impl	1,106	0,138
dotnet_shift_right_by_23	signed64	signed64	sync	impl	1,189	0,136
dotnet_shift_left_by_23	unsigned32	unsigned32	sync	impl	0,944	0,121
dotnet_shift_left_by_23	signed32	signed32	sync	impl	0,944	0,121
dotnet_shift_right_by_23	unsigned32	unsigned32	sync	impl	0,806	0,154
dotnet_shift_right_by_23	signed32	signed32	sync	impl	1,355	0,138
dotnet_shift_left_by_24	unsigned64	unsigned64	sync	impl	0,968	0,136
dotnet_shift_left_by_24	signed64	signed64	sync	impl	0,968	0,136
dotnet_shift_right_by_24	unsigned64	unsigned64	sync	impl	1,468	0,130
dotnet_shift_right_by_24	signed64	signed64	sync	impl	1,115	0,108
dotnet_shift_left_by_24	unsigned32	unsigned32	sync	impl	0,965	0,136
dotnet_shift_left_by_24	signed32	signed32	sync	impl	0,965	0,136
dotnet_shift_right_by_24	unsigned32	unsigned32	sync	impl	0,945	0,119
dotnet_shift_right_by_24	signed32	signed32	sync	impl	1,186	0,142
dotnet_shift_left_by_25	unsigned64	unsigned64	sync	impl	0,953	0,123
dotnet_shift_left_by_25	signed64	signed64	sync	impl	0,953	0,123
dotnet_shift_right_by_25	unsigned64	unsigned64	sync	impl	1,106	0,136
dotnet_shift_right_by_25	signed64	signed64	sync	impl	1,128	0,126
dotnet_shift_left_by_25	unsigned32	unsigned32	sync	impl	0,945	0,118
dotnet_shift_left_by_25	signed32	signed32	sync	impl	0,945	0,118
dotnet_shift_right_by_25	unsigned32	unsigned32	sync	impl	0,943	0,124
dotnet_shift_right_by_25	signed32	signed32	sync	impl	1,130	0,095
dotnet_shift_left_by_26	unsigned64	unsigned64	sync	impl	0,965	0,139
dotnet_shift_left_by_26	signed64	signed64	sync	impl	0,965	0,139
dotnet_shift_right_by_26	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_right_by_26	signed64	signed64	sync	impl	1,218	0,091
dotnet_shift_left_by_26	unsigned32	unsigned32	sync	impl	0,806	0,154
dotnet_shift_left_by_26	signed32	signed32	sync	impl	0,806	0,154
dotnet_shift_right_by_26	unsigned32	unsigned32	sync	impl	1,084	0,121
dotnet_shift_right_by_26	signed32	signed32	sync	impl	0,945	0,118
dotnet_shift_left_by_27	unsigned64	unsigned64	sync	impl	1,122	0,147
dotnet_shift_left_by_27	signed64	signed64	sync	impl	1,122	0,147
dotnet_shift_right_by_27	unsigned64	unsigned64	sync	impl	0,966	0,136
dotnet_shift_right_by_27	signed64	signed64	sync	impl	1,240	0,187
dotnet_shift_left_by_27	unsigned32	unsigned32	sync	impl	0,806	0,158
dotnet_shift_left_by_27	signed32	signed32	sync	impl	0,806	0,158
dotnet_shift_right_by_27	unsigned32	unsigned32	sync	impl	0,806	0,155
dotnet_shift_right_by_27	signed32	signed32	sync	impl	1,272	0,123
dotnet_shift_left_by_28	unsigned64	unsigned64	sync	impl	1,074	0,110
dotnet_shift_left_by_28	signed64	signed64	sync	impl	1,074	0,110
dotnet_shift_right_by_28	unsigned64	unsigned64	sync	impl	0,949	0,121
dotnet_shift_right_by_28	signed64	signed64	sync	impl	1,950	0,108
dotnet_shift_left_by_28	unsigned32	unsigned32	sync	impl	0,806	0,154
dotnet_shift_left_by_28	signed32	signed32	sync	impl	0,806	0,154
dotnet_shift_right_by_28	unsigned32	unsigned32	sync	impl	0,761	0,154
dotnet_shift_right_by_28	signed32	signed32	sync	impl	0,960	0,153
dotnet_shift_left_by_29	unsigned64	unsigned64	sync	impl	0,969	0,138
dotnet_shift_left_by_29	signed64	signed64	sync	impl	0,969	0,138
dotnet_shift_right_by_29	unsigned64	unsigned64	sync	impl	1,480	0,104
dotnet_shift_right_by_29	signed64	signed64	sync	impl	1,115	0,139
dotnet_shift_left_by_29	unsigned32	unsigned32	sync	impl	0,650	0,134
dotnet_shift_left_by_29	signed32	signed32	sync	impl	0,650	0,134
dotnet_shift_right_by_29	unsigned32	unsigned32	sync	impl	0,773	0,153
dotnet_shift_right_by_29	signed32	signed32	sync	impl	1,181	0,128
dotnet_shift_left_by_30	unsigned64	unsigned64	sync	impl	1,187	0,121
dotnet_shift_left_by_30	signed64	signed64	sync	impl	1,187	0,121
dotnet_shift_right_by_30	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_right_by_30	signed64	signed64	sync	impl	1,188	0,122
dotnet_shift_left_by_30	unsigned32	unsigned32	sync	impl	0,671	0,154
dotnet_shift_left_by_30	signed32	signed32	sync	impl	0,671	0,154
dotnet_shift_right_by_30	unsigned32	unsigned32	sync	impl	0,606	0,138
dotnet_shift_right_by_30	signed32	signed32	sync	impl	1,247	0,121
dotnet_shift_left_by_31	unsigned64	unsigned64	sync	impl	0,972	0,139
dotnet_shift_left_by_31	signed64	signed64	sync	impl	0,972	0,139
dotnet_shift_right_by_31	unsigned64	unsigned64	sync	impl	0,997	0,140
dotnet_shift_right_by_31	signed64	signed64	sync	impl	1,828	0,134
dotnet_shift_left_by_31	unsigned32	unsigned32	sync	impl	0,660	0,138
dotnet_shift_left_by_31	signed32	signed32	sync	impl	0,660	0,138
dotnet_shift_right_by_31	unsigned32	unsigned32	sync	impl	0,653	0,136
dotnet_shift_right_by_31	signed32	signed32	sync	impl	1,260	0,018
dotnet_shift_left_by_32	unsigned64	unsigned64	sync	impl	1,084	0,123
dotnet_shift_left_by_32	signed64	signed64	sync	impl	1,084	0,123
dotnet_shift_right_by_32	unsigned64	unsigned64	sync	impl	0,996	0,163
dotnet_shift_right_by_32	signed64	signed64	sync	impl	1,492	0,097
dotnet_shift_left_by_33	unsigned64	unsigned64	sync	impl	0,955	0,116
dotnet_shift_left_by_33	signed64	signed64	sync	impl	0,955	0,116
dotnet_shift_right_by_33	unsigned64	unsigned64	sync	impl	0,949	0,121
dotnet_shift_right_by_33	signed64	signed64	sync	impl	2,358	0,183
dotnet_shift_left_by_34	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_left_by_34	signed64	signed64	sync	impl	0,965	0,135
dotnet_shift_right_by_34	unsigned64	unsigned64	sync	impl	1,053	0,145
dotnet_shift_right_by_34	signed64	signed64	sync	impl	1,292	0,133
dotnet_shift_left_by_35	unsigned64	unsigned64	sync	impl	1,182	0,127
dotnet_shift_left_by_35	signed64	signed64	sync	impl	1,182	0,127
dotnet_shift_right_by_35	unsigned64	unsigned64	sync	impl	0,965	0,136
dotnet_shift_right_by_35	signed64	signed64	sync	impl	2,193	0,026
dotnet_shift_left_by_36	unsigned64	unsigned64	sync	impl	0,955	0,120
dotnet_shift_left_by_36	signed64	signed64	sync	impl	0,955	0,120
dotnet_shift_right_by_36	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_right_by_36	signed64	signed64	sync	impl	1,737	0,133
dotnet_shift_left_by_37	unsigned64	unsigned64	sync	impl	1,718	0,048
dotnet_shift_left_by_37	signed64	signed64	sync	impl	1,718	0,048
dotnet_shift_right_by_37	unsigned64	unsigned64	sync	impl	0,945	0,118
dotnet_shift_right_by_37	signed64	signed64	sync	impl	2,072	0,138
dotnet_shift_left_by_38	unsigned64	unsigned64	sync	impl	1,111	0,109
dotnet_shift_left_by_38	signed64	signed64	sync	impl	1,111	0,109
dotnet_shift_right_by_38	unsigned64	unsigned64	sync	impl	0,965	0,136
dotnet_shift_right_by_38	signed64	signed64	sync	impl	1,337	0,142
dotnet_shift_left_by_39	unsigned64	unsigned64	sync	impl	0,965	0,138
dotnet_shift_left_by_39	signed64	signed64	sync	impl	0,965	0,138
dotnet_shift_right_by_39	unsigned64	unsigned64	sync	impl	1,040	0,149
dotnet_shift_right_by_39	signed64	signed64	sync	impl	1,494	0,079
dotnet_shift_left_by_40	unsigned64	unsigned64	sync	impl	0,944	0,124
dotnet_shift_left_by_40	signed64	signed64	sync	impl	0,944	0,124
dotnet_shift_right_by_40	unsigned64	unsigned64	sync	impl	1,011	0,140
dotnet_shift_right_by_40	signed64	signed64	sync	impl	1,588	0,100
dotnet_shift_left_by_41	unsigned64	unsigned64	sync	impl	0,966	0,136
dotnet_shift_left_by_41	signed64	signed64	sync	impl	0,966	0,136
dotnet_shift_right_by_41	unsigned64	unsigned64	sync	impl	0,966	0,137
dotnet_shift_right_by_41	signed64	signed64	sync	impl	2,331	0,022
dotnet_shift_left_by_42	unsigned64	unsigned64	sync	impl	1,086	0,121
dotnet_shift_left_by_42	signed64	signed64	sync	impl	1,086	0,121
dotnet_shift_right_by_42	unsigned64	unsigned64	sync	impl	1,093	0,119
dotnet_shift_right_by_42	signed64	signed64	sync	impl	1,516	0,142
dotnet_shift_left_by_43	unsigned64	unsigned64	sync	impl	0,945	0,118
dotnet_shift_left_by_43	signed64	signed64	sync	impl	0,945	0,118
dotnet_shift_right_by_43	unsigned64	unsigned64	sync	impl	0,945	0,118
dotnet_shift_right_by_43	signed64	signed64	sync	impl	1,241	0,148
dotnet_shift_left_by_44	unsigned64	unsigned64	sync	impl	1,012	0,141
dotnet_shift_left_by_44	signed64	signed64	sync	impl	1,012	0,141
dotnet_shift_right_by_44	unsigned64	unsigned64	sync	impl	1,011	0,141
dotnet_shift_right_by_44	signed64	signed64	sync	impl	1,340	0,047
dotnet_shift_left_by_45	unsigned64	unsigned64	sync	impl	0,945	0,118
dotnet_shift_left_by_45	signed64	signed64	sync	impl	0,945	0,118
dotnet_shift_right_by_45	unsigned64	unsigned64	sync	impl	0,966	0,136
dotnet_shift_right_by_45	signed64	signed64	sync	impl	2,052	0,108
dotnet_shift_left_by_46	unsigned64	unsigned64	sync	impl	0,964	0,135
dotnet_shift_left_by_46	signed64	signed64	sync	impl	0,964	0,135
dotnet_shift_right_by_46	unsigned64	unsigned64	sync	impl	1,508	0,133
dotnet_shift_right_by_46	signed64	signed64	sync	impl	2,955	0,107
dotnet_shift_left_by_47	unsigned64	unsigned64	sync	impl	0,964	0,138
dotnet_shift_left_by_47	signed64	signed64	sync	impl	0,964	0,138
dotnet_shift_right_by_47	unsigned64	unsigned64	sync	impl	0,944	0,120
dotnet_shift_right_by_47	signed64	signed64	sync	impl	2,351	0,138
dotnet_shift_left_by_48	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_left_by_48	signed64	signed64	sync	impl	0,965	0,135
dotnet_shift_right_by_48	unsigned64	unsigned64	sync	impl	0,966	0,136
dotnet_shift_right_by_48	signed64	signed64	sync	impl	1,659	0,065
dotnet_shift_left_by_49	unsigned64	unsigned64	sync	impl	0,965	0,135
dotnet_shift_left_by_49	signed64	signed64	sync	impl	0,965	0,135
dotnet_shift_right_by_49	unsigned64	unsigned64	sync	impl	0,964	0,137
dotnet_shift_right_by_49	signed64	signed64	sync	impl	1,235	0,157
dotnet_shift_left_by_50	unsigned64	unsigned64	sync	impl	0,945	0,118
dotnet_shift_left_by_50	signed64	signed64	sync	impl	0,945	0,118
dotnet_shift_right_by_50	unsigned64	unsigned64	sync	impl	1,159	0,132
dotnet_shift_right_by_50	signed64	signed64	sync	impl	2,211	0,024
dotnet_shift_left_by_51	unsigned64	unsigned64	sync	impl	0,944	0,121
dotnet_shift_left_by_51	signed64	signed64	sync	impl	0,944	0,121
dotnet_shift_right_by_51	unsigned64	unsigned64	sync	impl	0,965	0,136
dotnet_shift_right_by_51	signed64	signed64	sync	impl	3,014	0,069
dotnet_shift_left_by_52	unsigned64	unsigned64	sync	impl	0,945	0,118
dotnet_shift_left_by_52	signed64	signed64	sync	impl	0,945	0,118
dotnet_shift_right_by_52	unsigned64	unsigned64	sync	impl	0,964	0,137
dotnet_shift_right_by_52	signed64	signed64	sync	impl	1,887	0,064
dotnet_shift_left_by_53	unsigned64	unsigned64	sync	impl	0,814	0,154
dotnet_shift_left_by_53	signed64	signed64	sync	impl	0,814	0,154
dotnet_shift_right_by_53	unsigned64	unsigned64	sync	impl	0,806	0,153
dotnet_shift_right_by_53	signed64	signed64	sync	impl	1,925	0,067
dotnet_shift_left_by_54	unsigned64	unsigned64	sync	impl	0,945	0,120
dotnet_shift_left_by_54	signed64	signed64	sync	impl	0,945	0,120
dotnet_shift_right_by_54	unsigned64	unsigned64	sync	impl	0,964	0,137
dotnet_shift_right_by_54	signed64	signed64	sync	impl	2,330	0,113
dotnet_shift_left_by_55	unsigned64	unsigned64	sync	impl	0,942	0,121
dotnet_shift_left_by_55	signed64	signed64	sync	impl	0,942	0,121
dotnet_shift_right_by_55	unsigned64	unsigned64	sync	impl	0,806	0,154
dotnet_shift_right_by_55	signed64	signed64	sync	impl	1,474	0,081
dotnet_shift_left_by_56	unsigned64	unsigned64	sync	impl	0,790	0,125
dotnet_shift_left_by_56	signed64	signed64	sync	impl	0,790	0,125
dotnet_shift_right_by_56	unsigned64	unsigned64	sync	impl	1,627	0,123
dotnet_shift_right_by_56	signed64	signed64	sync	impl	2,322	0,032
dotnet_shift_left_by_57	unsigned64	unsigned64	sync	impl	0,755	0,134
dotnet_shift_left_by_57	signed64	signed64	sync	impl	0,755	0,134
dotnet_shift_right_by_57	unsigned64	unsigned64	sync	impl	0,964	0,137
dotnet_shift_right_by_57	signed64	signed64	sync	impl	2,641	0,123
dotnet_shift_left_by_58	unsigned64	unsigned64	sync	impl	0,965	0,138
dotnet_shift_left_by_58	signed64	signed64	sync	impl	0,965	0,138
dotnet_shift_right_by_58	unsigned64	unsigned64	sync	impl	0,806	0,154
dotnet_shift_right_by_58	signed64	signed64	sync	impl	1,253	0,106
dotnet_shift_left_by_59	unsigned64	unsigned64	sync	impl	0,806	0,153
dotnet_shift_left_by_59	signed64	signed64	sync	impl	0,806	0,153
dotnet_shift_right_by_59	unsigned64	unsigned64	sync	impl	0,810	0,154
dotnet_shift_right_by_59	signed64	signed64	sync	impl	2,085	0,061
dotnet_shift_left_by_60	unsigned64	unsigned64	sync	impl	0,792	0,113
dotnet_shift_left_by_60	signed64	signed64	sync	impl	0,792	0,113
dotnet_shift_right_by_60	unsigned64	unsigned64	sync	impl	0,814	0,154
dotnet_shift_right_by_60	signed64	signed64	sync	impl	2,122	0,047
dotnet_shift_left_by_61	unsigned64	unsigned64	sync	impl	0,671	0,152
dotnet_shift_left_by_61	signed64	signed64	sync	impl	0,671	0,152
dotnet_shift_right_by_61	unsigned64	unsigned64	sync	impl	0,670	0,153
dotnet_shift_right_by_61	signed64	signed64	sync	impl	2,691	0,061
dotnet_shift_left_by_62	unsigned64	unsigned64	sync	impl	0,670	0,153
dotnet_shift_left_by_62	signed64	signed64	sync	impl	0,670	0,153
dotnet_shift_right_by_62	unsigned64	unsigned64	sync	impl	0,806	0,153
dotnet_shift_right_by_62	signed64	signed64	sync	impl	2,113	0,080
dotnet_shift_left_by_63	unsigned64	unsigned64	sync	impl	0,671	0,154
dotnet_shift_left_by_63	signed64	signed64	sync	impl	0,671	0,154
dotnet_shift_right_by_63	unsigned64	unsigned64	sync	impl	0,651	0,138
dotnet_shift_right_by_63	signed64	signed64	sync	impl	1,573	0,077
dotnet_shift_left	unsigned32	unsigned32	sync	impl	2,926	0,120
dotnet_shift_left	signed32	signed32	sync	impl	2,926	0,120
dotnet_shift_left	unsigned64	unsigned64	sync	impl	4,260	0,106
dotnet_shift_left	signed64	signed64	sync	impl	4,260	0,106
dotnet_shift_right	unsigned32	unsigned32	sync	impl	3,257	0,109
dotnet_shift_right	signed32	signed32	sync	impl	3,371	0,098
dotnet_shift_right	unsigned64	unsigned64	sync	impl	3,884	0,088
dotnet_shift_right	signed64	signed64	sync	impl	3,883	0,112
";
                        _timingReport = _timingReportParser.Parse(timingReport);
                    }

                    return _timingReport;
                }
            }
        }


        public CatapultDriver(ITimingReportParser timingReportParser)
        {
            _timingReportParser = timingReportParser;
        }


        public decimal GetClockCyclesNeededForBinaryOperation(BinaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForBinaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);

        public decimal GetClockCyclesNeededForUnaryOperation(UnaryOperatorExpression expression, int operandSizeBits, bool isSigned) =>
            DeviceDriverHelper.ComputeClockCyclesForUnaryOperation(DeviceManifest, TimingReport, expression, operandSizeBits, isSigned);
    }
}
