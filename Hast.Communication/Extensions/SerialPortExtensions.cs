using Orchard.Environment.Extensions;

namespace System.IO.Ports
{
    [OrchardFeature("Hast.Communication.Serial")]
    public static class SerialPortExtensions
    {
        public static void Write(this SerialPort serialPort, char character)
        {
            serialPort.Write(new[] { character }, 0, 1);
        }
    }
}
