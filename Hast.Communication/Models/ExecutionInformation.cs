using System;

namespace Hast.Communication.Models
{
    public class ExecutionInformation : IExecutionInformation
    {
        public long FpgaExecutionTime { get; set; }
        public long FullExecutionTimeMilliseconds { get; set; }
        public DateTime StartedUtc { get; set; }


        public ExecutionInformation()
        {
            StartedUtc = DateTime.UtcNow;
            FpgaExecutionTime = 0;
            FullExecutionTimeMilliseconds = 0;
        }
    }
}
