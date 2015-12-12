using System.IO.Ports;

namespace Hast.Communication.Constants
{
    public static class SerialCommunicationConstants
    {
        public const int BaudRate = 9600;
        public const Parity SerialPortParity = Parity.None;
        public const StopBits SerialPortStopBits = StopBits.One;
        public const int WriteTimeoutInMilliseconds = 10000;

        public static class Signals
        {
            public const string Ready = "s";
            public const string AllBytesReceived = "r";
            public const char Yes = 'y';
            public const string FpgaDetect = "p";
            public const char Information = 'i';
            public const char Result = 'd';
            public const char Default = '0';
        }
    }
}
