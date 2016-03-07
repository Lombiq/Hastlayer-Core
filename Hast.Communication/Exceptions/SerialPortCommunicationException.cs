using Orchard.Environment.Extensions;
using System;

namespace Hast.Communication.Exceptions
{
    /// <summary>
    /// This exception is thrown when something is wrong with the FPGA board connected through serial connection.
    /// </summary>
    [OrchardFeature("Hast.Communication.Serial")]
    public class SerialPortCommunicationException : Exception
    {
        public SerialPortCommunicationException(string message) : base(message) { }

        public SerialPortCommunicationException(string message, Exception inner) : base(message, inner) { }
    }
}
