using System;

namespace Hast.Remote.Worker.Models
{
    public class TransformationTelemetry
    {
        public string JobName { get; set; }
        public int AppId { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime FinishTimeUtc { get; set; }
        public bool IsSuccess { get; set; }
    }
}
