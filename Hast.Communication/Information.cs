using System;

namespace Hast.Communication
{
    public class Information
    {
        // The useful information received.
        public string Message { get; set; }
        public DateTime ReceivedDate { get; set; }


        public Information(string message)
        {
            this.ReceivedDate = DateTime.Now;
            this.Message = message;
        }


        public override string ToString()
        {
            return string.Format("Information received at {0}: {1}", ReceivedDate, Message);
        }
    }
}
