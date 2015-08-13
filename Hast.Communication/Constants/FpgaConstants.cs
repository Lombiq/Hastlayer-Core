using System.IO.Ports;

namespace Hast.Communication.Constants
{
    public class FpgaConstants
    {
        public const string PortName = "COM4";
        public const int BaudRate = 115200;
        public const Parity SerialPortParity = Parity.None;
        public const StopBits SerialPortStopBits = StopBits.One;
        public const int WriteTimeout = 10000;
    }
}
