using System.IO.Ports;
using Hast.Common.Extensibility.Pipeline;
using Hast.Communication.Extensibility.Pipeline;

namespace Hast.Xilinx
{
    public class SerialPortConfigurator : PipelineStepBase, ISerialPortConfigurator
    {
        public void ConfigureSerialPort(SerialPort serialPort)
        {
            serialPort.BaudRate = 230400;
        }
    }
}
