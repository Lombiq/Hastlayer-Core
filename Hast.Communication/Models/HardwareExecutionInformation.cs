using System;
using Hast.Common.Models;

namespace Hast.Communication.Models
{
    public class HardwareExecutionInformation : IHardwareExecutionInformation
    {
        public ulong HardwareExecutionTimeMilliseconds { get; set; }
        public long FullExecutionTimeMilliseconds { get; set; }
        public DateTime StartedUtc { get; set; }


        public HardwareExecutionInformation()
        {
            StartedUtc = DateTime.UtcNow;
            HardwareExecutionTimeMilliseconds = 0;
            FullExecutionTimeMilliseconds = 0;
        }
    }
}
