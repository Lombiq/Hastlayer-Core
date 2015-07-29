using System;

namespace Hast.Communication.Exceptions
{
    /// <summary>
    /// This exceptin is thrown when something is wrong with the connected FPGA board.
    /// </summary>
    class SerialPortCommunicationException : Exception
    {
        public SerialPortCommunicationException(string message) : base(message) { }
        public SerialPortCommunicationException(string message, Exception inner) : base(message, inner){ }
    }
}
