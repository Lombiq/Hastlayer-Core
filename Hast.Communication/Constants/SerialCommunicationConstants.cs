using Orchard.Environment.Extensions;
using System.IO.Ports;

namespace Hast.Communication.Constants
{
    [OrchardFeature("Hast.Communication.Serial")]
    internal static class SerialCommunicationConstants
    {
        public const int BaudRate = 9600;
        public const Parity SerialPortParity = Parity.None;
        public const StopBits SerialPortStopBits = StopBits.One;
        public const int WriteTimeoutInMilliseconds = 10000;
        public const string ChannelName = "SerialPort";


        public static class Signals
        {
            public const char Echo = 'e';
            public const char Ping = 'p';
            public const char Ready = 'r';
        }


        public enum CommunicationState
        {
            WaitForFirstResponse,
            ReceivingExecutionInformation,
            ReceivingOutputByteCount,
            ReceivingOuput,
            Finished
        }
    }
}
