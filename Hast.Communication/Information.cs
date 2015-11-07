using System;

namespace Hast.Communication
{
    /// <summary>
    /// For measuring the exection times.
    /// </summary>
    public class Information
    {
        /// <summary>
        /// The exection time received from the FPGA board.
        /// </summary>
        public long FpgaExecutionTime { get; set; }
        /// <summary>
        /// The full exection time.
        /// </summary>
        public long FullExecutionTime { get; set; }
        /// <summary>
        /// The date when the exection started.
        /// </summary>
        public DateTime Started { get; set; }


        public Information()
        {
            this.Started = DateTime.Now;
            this.FpgaExecutionTime = 0;
            this.FullExecutionTime = 0;
        }
    }
}
