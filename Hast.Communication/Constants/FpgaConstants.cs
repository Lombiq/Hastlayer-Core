using System.IO.Ports;

namespace Hast.Communication.Constants
{
    public class FpgaConstants
    {
        public const string PortName = "COM4";
        public const int BaudRate = 9600;
        public const Parity SerialPortParity = Parity.None;
        public const StopBits SerialPortStopBits = StopBits.One;
        public const int WriteTimeout = 10000;
        public const string signalReady = "s"; 
        public const string signalAllBytesReceived = "r";
        public const char signalYes = 'y'; // This constatnt is used for ACK or positive answer from the FPGA. (yes)
        public const string signalFpgaDetect = "p";
    }
}
