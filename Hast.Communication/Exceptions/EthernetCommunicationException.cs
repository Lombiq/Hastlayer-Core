using Orchard.Environment.Extensions;
using System;

namespace Hast.Communication.Exceptions
{
    /// <summary>
    /// This exception is thrown when something is wrong with the FPGA board connected through ethernet connection.
    /// </summary>
    [OrchardFeature("Hast.Communication.Ethernet")]
    public class EthernetCommunicationException : Exception
    {
        public EthernetCommunicationException(string message) : base(message) { }

        public EthernetCommunicationException(string message, Exception inner) : base(message, inner) { }
    }
}
