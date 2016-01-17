using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.IO.Ports
{
    public static class SerialPortExtensions
    {
        public static void Write(this SerialPort serialPort, char character)
        {
            serialPort.Write(new[] { character }, 0, 1);
        }
    }
}
