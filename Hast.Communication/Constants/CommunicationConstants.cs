using System.IO.Ports;

namespace Hast.Communication.Constants
{
    public static class CommunicationConstants
    {
        public const string DefaultChannelName = Ethernet.ChannelName;


        public static class Ethernet
        {
            public const string ChannelName = "Ethernet";


            public static class Ports
            {
                public const int WhoIsAvailableRequest = 34050;
                public const int WhoIsAvailableResponse = 33000;
            }


            public static class Signals
            {
                public const char Busy = 'b';
                public const char Ready = 'r';
            }
        }

        public static class Serial
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
}
