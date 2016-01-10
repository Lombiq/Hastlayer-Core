using System;
using Hast.Common.Models;

namespace Hast.Communication.Models
{
    public class HardwareExecutionInformation : IHardwareExecutionInformation
    {
        public long FpgaExecutionTime { get; set; }
        public long FullExecutionTimeMilliseconds { get; set; }
        public DateTime StartedUtc { get; set; }


        public HardwareExecutionInformation()
        {
            StartedUtc = DateTime.UtcNow;
            FpgaExecutionTime = 0;
            FullExecutionTimeMilliseconds = 0;
        }
    }
}
