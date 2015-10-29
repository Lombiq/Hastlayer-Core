using System.IO.Ports;

namespace Hast.Communication.Constants
{
    public class FpgaConstants
    {
        public const string PortName = "COM4";
        public const int BaudRate = 9600;
        public const Parity SerialPortParity = Parity.None;
        public const StopBits SerialPortStopBits = StopBits.One;
        public const int WriteTimeoutInMilliseconds = 10000;
        public const string SignalReady = "s"; 
        public const string SignalAllBytesReceived = "r";
        public const char SignalYes = 'y'; // This constant is used for recognizing the positive answer coming from the FPGA. (yes)
        public const string SignalFpgaDetect = "p";
        public const char SignalInformation = 'i';
        public const char SignalData = 'd';
        public const char SignalDefault = '0';
    }
}
